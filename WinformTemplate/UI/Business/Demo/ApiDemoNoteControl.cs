using WinformTemplate.Business.Demo.Repositories;
using WinformTemplate.Business.Demo.ViewModel;

namespace WinformTemplate.UI.Business.Demo;

public sealed class ApiDemoNoteControl : DemoNoteControlBase
{
    public ApiDemoNoteControl(ApiDemoNoteRepository repository)
        : base(new DemoNoteManagementViewModel(repository, "WebAPI"))
    {
    }
}
