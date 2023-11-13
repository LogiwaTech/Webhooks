using AutoMapper;
using AutoMapper.Internal;

namespace Shared.Kernel.AutoMapper
{
    public static class WithProfile<TProfile> where TProfile : Profile, new()
    {
        private static readonly Lazy<IMapper> MapperFactory = new Lazy<IMapper>(() =>
        {
            var mapperConfig = new MapperConfiguration(config => config.AddProfile<TProfile>());
            return new Mapper(mapperConfig, ServiceCtor ?? ((IGlobalConfiguration)mapperConfig).ServiceCtor);
        });        

        public static IMapper Mapper => MapperFactory.Value;

        public static Func<Type, object> ServiceCtor { get; set; }

        public static TDestination Map<TDestination>(object source)
        {
            return Mapper.Map<TDestination>(source);
        }
    }
}
