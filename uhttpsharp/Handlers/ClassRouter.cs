using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace uhttpsharp.Handlers
{
    public class ClassRouter : IHttpRequestHandler
    {
        private static readonly ConcurrentDictionary<Tuple<Type, string>, Func<IHttpRequestHandler, IHttpRequestHandler>>
            Routers = new ConcurrentDictionary<Tuple<Type, string>, Func<IHttpRequestHandler, IHttpRequestHandler>>();

        private static readonly ConcurrentDictionary<Type, ICollection<string>> PropertySet =
            new ConcurrentDictionary<Type, ICollection<string>>();

        private static readonly ConcurrentDictionary<Type, Func<IHttpRequestHandler, string, IHttpRequestHandler>>
            IndexerRouters = new ConcurrentDictionary<Type, Func<IHttpRequestHandler, string, IHttpRequestHandler>>();

        private IHttpRequestHandler _root;

        public ClassRouter(IHttpRequestHandler root)
        {
            _root = root;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            var handler = _root;

            foreach (var parameter in context.Request.RequestParameters)
            {

                var getNextHandler = Routers.GetOrAdd(Tuple.Create(handler.GetType(), parameter), CreateRoute);

                if (getNextHandler != null)
                {
                    handler = getNextHandler(handler);
                }
                else if (!TryGetNextByIndex(parameter, ref handler))
                {
                    return next();
                }

                // Incase that one of the methods returned null (Indexer / Getter)
                if (handler == null)
                {
                    return next();
                }
            }

            return handler.Handle(context, next);
        }
        private bool TryGetNextByIndex(string parameter, ref IHttpRequestHandler handler)
        {
            var getNextByIndex = IndexerRouters.GetOrAdd(handler.GetType(), GetIndexerRouter);

            if (getNextByIndex == null)
            {
                return false;
            }

            handler = getNextByIndex(handler, parameter);
            return true;
        }
        private Func<IHttpRequestHandler, string, IHttpRequestHandler> GetIndexerRouter(Type arg)
        {
            var indexer = GetIndexer(arg);

            if (indexer == null)
            {
                return null;
            }
            var parameterType = indexer.GetIndexParameters()[0].ParameterType;

            var inputHandler = Expression.Parameter(typeof(IHttpRequestHandler));
            var inputObject = Expression.Parameter(typeof(string));

            var tryParseMethod = parameterType.GetMethod("TryParse", new[] { typeof(string), parameterType.MakeByRefType() });

            Expression body;

            if (tryParseMethod == null)
            {
                var handlerConverted = Expression.Convert(inputHandler, arg);
                var objectConverted =
                    Expression.Convert(
                        Expression.Call(typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) }), inputObject,
                            Expression.Constant(parameterType)), parameterType);

                var indexerExpression = Expression.Property(handlerConverted, indexer, objectConverted);
                var returnValue = Expression.Convert(indexerExpression, typeof(IHttpRequestHandler));

                body = returnValue;
            }
            else
            {
                var inputConvertedVar = Expression.Variable(parameterType, "inputObjectConverted");

                var handlerConverted = Expression.Convert(inputHandler, arg);
                var objectConverted = inputConvertedVar;

                var indexerExpression = Expression.Property(handlerConverted, indexer, objectConverted);
                var returnValue = Expression.Convert(indexerExpression, typeof(IHttpRequestHandler));
                var returnTarget = Expression.Label(typeof(IHttpRequestHandler));
                var returnLabel = Expression.Label(returnTarget, Expression.Convert(Expression.Constant(null), typeof(IHttpRequestHandler)));
                body =
                    Expression.Block(
                    new[] { inputConvertedVar },
                        Expression.IfThenElse(
                        Expression.Call(tryParseMethod, inputObject,
                            inputConvertedVar),
                        Expression.Return(returnTarget, returnValue),
                        Expression.Constant(null)
                        ),
                        returnLabel);
            }


            return Expression.Lambda<Func<IHttpRequestHandler, string, IHttpRequestHandler>>(body, inputHandler,
                inputObject).Compile();
        }
        private PropertyInfo GetIndexer(Type arg)
        {
            var indexer = arg.GetProperties()
                .SingleOrDefault(p => p.GetIndexParameters().Length == 1 &&
                                      typeof(IHttpRequestHandler).IsAssignableFrom(p.PropertyType));

            return indexer;
        }
        private ICollection<string> GetPropertySet(Type arg)
        {
            return new HashSet<string>(arg.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name));
        }
        private Func<IHttpRequestHandler, IHttpRequestHandler> CreateRoute(Tuple<Type, string> arg)
        {
            var parameter = Expression.Parameter(typeof(IHttpRequestHandler), "input");
            var converted = Expression.Convert(parameter, arg.Item1);

            var propertyInfo = arg.Item1.GetProperty(arg.Item2);

            if (propertyInfo == null)
            {
                return null;
            }

            var property = Expression.Property(converted, propertyInfo);
            var propertyConverted = Expression.Convert(property, typeof(IHttpRequestHandler));

            return Expression.Lambda<Func<IHttpRequestHandler, IHttpRequestHandler>>(propertyConverted, new[] { parameter }).Compile();
        }
    }
}
