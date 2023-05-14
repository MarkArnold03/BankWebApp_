using AutoMapper;

namespace BankWebApp.Infrastructure.Profiles
{
    public class AccountProfile : Profile
    {

        public AccountProfile()
        {
            CreateMap<BankAppData.Account, BankWebApp.ViewModel.AccountViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AccountId))
            .ReverseMap();

            CreateMap<BankAppData.Account, BankWebApp.ViewModel.AccountViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AccountId))
            .ReverseMap();

            CreateMap<BankAppData.Transaction, BankWebApp.Pages.Accounts.AccountsModel.TransactionViewModel>()
           .ReverseMap();

            CreateMap<BankAppData.Transaction, BankWebApp.Pages.Transactions.TransactionModel.TransactionViewModel>()
          .ReverseMap();
        }
    }
}
