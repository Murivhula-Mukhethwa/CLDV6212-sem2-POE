using ABC_retailers.Models;
using ABC_retailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_retailers.Controllers;

public class UploadController : Controller
{
    private readonly IFunctionsApi _functionsApi;

    public UploadController(IFunctionsApi functionsApi)
    {
        _functionsApi = functionsApi;
    }

    public IActionResult Index() => View(new FileUploadModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(FileUploadModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (model.ProofOfPayment == null || model.ProofOfPayment.Length == 0)
        {
            ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
            return View(model);
        }

        try
        {
            var fileName = await _functionsApi.UploadProofOfPaymentAsync(
                model.ProofOfPayment,
                model.OrderId,
                model.CustomerName
            );

            TempData["Success"] = $"File uploaded successfully: {fileName}";
            return View(new FileUploadModel());
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
            return View(model);
        }
    }
}
