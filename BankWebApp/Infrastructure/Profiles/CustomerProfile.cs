using AutoMapper;
using BankWebApp.BankAppData;
using BankWebApp.Pages.Customers;
using BankWebApp.ViewModel;

namespace BankWebApp.Infrastructure.Profiles
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<BankAppData.Customer, BankWebApp.ViewModel.CustomersViewModel>()
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Givenname + " " + src.Surname))
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CustomerId))
           .ReverseMap();

            CreateMap<BankAppData.Customer, BankWebApp.ViewModel.CustomerCardViewModel>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.Givenname} {src.Surname}"))
            .ReverseMap();

        }
    }
}
