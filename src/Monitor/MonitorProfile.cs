using AutoMapper;
using MonitorLib.Dtos;
using MonitorLib.Enums;
using MonitorLib.Messages;

namespace Monitor
{
    public class MonitorProfile : Profile
    {
        public MonitorProfile()
        {
            CreateMap<HttpMonitor, CreateHttpMonitorMessage>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => MonitorType.Http));

            CreateMap<DnsMonitor, CreateDnsMonitorMessage>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => MonitorType.DNS));

            CreateMap<SlackAlert, CreateSlackAlertMessage>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => AlertType.Slack));

            CreateMap<AlertDetailsMessageRes, AlertDetails>();

            CreateMap<MonitorStatusMessageRes, MonitorStatus>();
        }
    }
}
