using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CKVideoNVRWeb.Services;
using CKVideoNVRWeb.Models;

namespace CKVideoNVRWeb.Pages
{
    public class DeleteModel : PageModel
    {
        private readonly CameraService _cameraService;

        public DeleteModel(CameraService cameraService)
        {
            _cameraService = cameraService;
        }

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

        public IActionResult OnPost(string id)
        {
            _cameraService.Delete(id);
            return RedirectToPage("/Index");
        }
    }
}