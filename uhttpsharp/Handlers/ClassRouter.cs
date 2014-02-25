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

        private static readonly ConcurrentDictionary<Type, Func<IHttpRequestHandler, string, Task<IHttpRequestHandler>>>
            IndexerRouters = new ConcurrentDictionary<Type, Func<IHttpRequestHandler, string, Task<IHttpRequestHandler>>>();

        private readonly IHttpRequestHandler _root;

        public ClassRouter(IHttpRequestHandler root)
        {
            _root = root;
        }

        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            var handler = _root;

            foreach (var parameter in context.Request.RequestParameters)
            {

                var getNextHandler = Routers.GetOrAdd(Tuple.Create(handler.GetType(), parameter), CreateRoute);

                if (getNextHandler != null)
                {
                    handler = getNextHandler(handler);
                }
                else
                {
                    var getNextByIndex = IndexerRouters.GetOrAdd(handler.GetType(), GetIndexerRouter);

                    if (getNextByIndex == null)
                    {
                        await next();
                        return;
                    }

                    var returnedTask = getNextByIndex(handler, parameter);

                    if (returnedTask == null)
                    {
                        await next();
                        return;
                    }

                    handler = await returnedTask;
                }

                // Incase that one of the methods returned null (Indexer / Getter)
                if (handler == null)
                {
                    await next();
                    return;
                }
            }

            await handler.Handle(context, next);
        }

        private Func<IHttpRequestHandler, string, Task<IHttpRequestHandler>> GetIndexerRouter(Type arg)
        {
            var indexer = GetIndexer(arg);

            if (indexer == null)
            {
                return null;
            }
            var parameterType = indexer.GetParameters()[0].ParameterType;

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

                var indexerExpression = Expression.Call(handlerConverted, indexer, objectConverted);
                var returnValue = Expression.Convert(indexerExpression, typeof(IHttpRequestHandler));

                body = returnValue;
            }
            else
            {
                var inputConvertedVar = Expression.Variable(parameterType, "inputObjectConverted");

                var handlerConverted = Expression.Convert(inputHandler, arg);
                var objectConverted = inputConvertedVar;

                var indexerExpression = Expression.Call(handlerConverted, indexer, objectConverted);
                var returnValue = Expression.Convert(indexerExpression, typeof(Task<IHttpRequestHandler>));
                var returnTarget = Expression.Label(typeof(Task<IHttpRequestHandler>));
                var returnLabel = Expression.Label(returnTarget, Expression.Convert(Expression.Constant(null), typeof(Task<IHttpRequestHandler>)));
                body =
                    Expression.Block(
                    new[] { inputConvertedVar },
                        Expression.IfThen(
                        Expression.Call(tryParseMethod, inputObject,
                            inputConvertedVar),
                        Expression.Return(returnTarget, returnValue)
                        ),
                        returnLabel);
            }


            return Expression.Lambda<Func<IHttpRequestHandler, string, Task<IHttpRequestHandler>>>(body, inputHandler,
                inputObject).Compile();
        }
        private MethodInfo GetIndexer(Type arg)
        {
            var indexer =
                arg.GetMethods().SingleOrDefault(m => Attribute.IsDefined(m, typeof(IndexerAttribute))
                                             && m.GetParameters().Length == 1
                                             && typeof(Task<IHttpRequestHandler>).IsAssignableFrom(m.ReturnType));

            return indexer;
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

    public class IndexerAttribute : Attribute
    {
    }
}
