using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CKVideoNVRWeb.Services;
using CKVideoNVRWeb.Models;

namespace CKVideoNVRWeb.Pages
{
    public class EditModel : PageModel
    {
        private readonly CameraService _cameraService;

        public EditModel(CameraService cameraService)
        {
            _cameraService = cameraService;
        }

        [BindProperty]
        public Camera Camera { get; set; }

        public IActionResult OnGet(string id)
        {
            Camera = _cameraService.GetById(id);
            if (Camera == null)
            {
                return NotFound();
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _cameraService.Update(Camera.Id, Camera);
            return RedirectToPage("/Index");
        }
    }
}