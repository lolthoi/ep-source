using Kloon.EmployeePerformance.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Extentions
{
    public static class ContextExtensions
    {
        /// <summary>
        /// Add scoped IEntityRepository, IContextFactory, IUnitOfWork
        /// </summary>
        /// <typeparam name="TContextFactory"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomizedDbContextFactory<TContextFactory>(this IServiceCollection services)
            where TContextFactory : class, IContextFactory
        {
            services.AddScoped(typeof(IEntityRepository<>), typeof(EntityRepository<>));
            services.AddScoped<IContextFactory, TContextFactory>();
            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

            return services;
        }
    }
}
