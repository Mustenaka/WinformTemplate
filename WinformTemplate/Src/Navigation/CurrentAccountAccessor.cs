using WinformTemplate.Business.Sys.Model;

namespace WinformTemplate.Navigation;

public sealed class CurrentAccountAccessor : ICurrentAccountAccessor
{
    public SysAccountModel? CurrentAccount { get; set; }
}
