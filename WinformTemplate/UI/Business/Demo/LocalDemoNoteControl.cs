using WinformTemplate.Business.Demo.Repositories;
using WinformTemplate.Business.Demo.ViewModel;

namespace WinformTemplate.UI.Business.Demo;

public sealed class LocalDemoNoteControl : DemoNoteControlBase
{
    public LocalDemoNoteControl(LocalDemoNoteRepository repository)
        : base(new DemoNoteManagementViewModel(repository, "Local"))
    {
    }
}
