using WinformTemplate.Business.Demo.Repositories;
using WinformTemplate.Business.Demo.ViewModel;

namespace WinformTemplate.UI.Business.Demo;

public sealed class EfDemoNoteControl : DemoNoteControlBase
{
    public EfDemoNoteControl(EfDemoNoteRepository repository)
        : base(new DemoNoteManagementViewModel(repository, "EF"))
    {
    }
}
