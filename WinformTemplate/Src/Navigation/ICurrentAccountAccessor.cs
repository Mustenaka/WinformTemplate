using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Navigation;

public interface ICurrentAccountAccessor
{
    SysAccountModel? CurrentAccount { get; set; }
}
