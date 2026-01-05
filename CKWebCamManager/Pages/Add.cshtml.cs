using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CKWebCamManager.Services;
using CKWebCamManager.Models;

namespace CKWebCamManager.Pages
{
    public class AddModel : PageModel
    {
        private readonly CameraService _cameraService;

        public AddModel(CameraService cameraService)
        {
            _cameraService = cameraService;
        }

        [BindProperty]
        public Camera Camera { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _cameraService.Add(Camera);
            return RedirectToPage("/Index");
        }
    }
}