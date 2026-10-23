using AutoMapper;

namespace BankWebApp.Infrastructure.Profiles
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<BankAppData.Customer, BankWebApp.ViewModel.CustomersViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Givenname + " " + src.Surname))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CustomerId));

            CreateMap<BankAppData.Customer, BankWebApp.ViewModel.CustomerCardViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.GivenName, opt => opt.MapFrom(src => src.Givenname))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Givenname + " " + src.Surname))
                .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.Streetaddress))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.Zipcode))
                .ForMember(dest => dest.TelephoneCountryCode, opt => opt.MapFrom(src => src.Telephonecountrycode))
                .ForMember(dest => dest.TelephoneNumber, opt => opt.MapFrom(src => src.Telephonenumber))
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.Emailaddress))
                .ForMember(dest => dest.Age, opt => opt.Ignore())
                .ForMember(dest => dest.Accounts, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAccountValue, opt => opt.Ignore());
        }
    }
}
