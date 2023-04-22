using AutoMapper;
using BankWebApp.BankAppData;
using BankWebApp.Pages.Customers;

namespace BankWebApp.Infrastructure.Profiles
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
             CreateMap<BankAppData.Customer, BankWebApp.Pages.Customers.CustomersModel.CustomersViewModel> ()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Givenname + " " + src.Surname))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CustomerId))
            .ReverseMap();
        }
    }
}
