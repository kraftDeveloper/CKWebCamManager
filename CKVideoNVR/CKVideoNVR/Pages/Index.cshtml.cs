using Microsoft.AspNetCore.Mvc.RazorPages;
using CKVideoNVRWeb.Services;
using CKVideoNVRWeb.Models;
using System.Collections.Generic;

namespace CKVideoNVRWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly CameraService _cameraService;

        public IndexModel(CameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public List<Camera> Cameras { get; set; }

        public void OnGet()
        {
            Cameras = _cameraService.GetAll();
        }
    }
}