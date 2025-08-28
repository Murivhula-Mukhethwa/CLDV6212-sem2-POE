using Microsoft.AspNetCore.Mvc;
using ABC_retailers.Models;
using ABC_retailers.Services;
using ABC_retailers.Services;

namespace ABC_retailers.Controllers
{
    public class UploadController : Controller
    {
        private readonly IAzureStorageService _storageService;

        public UploadController(IAzureStorageService storageService)
        {
            _storageService = storageService;

        }

        // GET: /Upload
        public IActionResult Index()
        {
            return View(new FileUploadModel());
        }

        // POST: /Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.ProofOfPayment != null && model.ProofOfPayment.Length > 0)
                    {

                        var fileName = await _storageService.UploadFileAsync(model.ProofOfPayment, "payment-proofs");

                        await _storageService.UploadToFileShareAsync(model.ProofOfPayment, "contracts", "payments");
                        TempData["Success"] = $"File uploaded successfully! File name :  {fileName}";

                        return View(new FileUploadModel());
                    }
                    else
                    {
                        ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                    }

                }
                catch (Exception ex)
                {

                    ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                }
            }
                return View(model);
            }
        }
    }
