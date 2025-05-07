using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Views.Shared.Components;

public class FooterViewComponent : ViewComponent
{
    private readonly DataContext _dataContext;

    public FooterViewComponent(DataContext context)
    {
        _dataContext = context;
    }

    public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Contacts.FirstOrDefaultAsync());
}
