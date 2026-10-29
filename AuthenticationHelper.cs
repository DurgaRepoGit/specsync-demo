using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text;

namespace specsync_demo.tests.Helpers
{
    public class AuthenticationHelper
    {
        public async Task LoginAsync(IPage page, string baseUrl, string username, string password)
        {
            await page.GotoAsync(baseUrl);
            await page.FillAsync("#username", username);
            await page.FillAsync("#password", password);
            await page.ClickAsync("#btn-login");
            await page.WaitForURLAsync("**/configurationdashboard");
        }
    }
}
