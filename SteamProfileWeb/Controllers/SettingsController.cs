using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SteamProfileWeb.ViewModels;
using System.IO;
using System;

public class SettingsController : Controller
{
    private readonly IFeaturesService featuresService;
    private readonly IUserService userService;

    public SettingsController(IFeaturesService featuresService, IUserService userService)
    {
        this.featuresService = featuresService;
        this.userService = userService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult ManageFeatures()
    {
        var model = featuresService.GetFeaturesByCategories();
        return View(model);
    }

    public IActionResult AccountSettings()
    {
        var user = userService.GetCurrentUser();
        var model = new AccountSettingsViewModel
        {
            Username = user?.Username,
            Email = user?.Email
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult AccountSettings(AccountSettingsViewModel model, string action)
    {
        var user = userService.GetCurrentUser();
        if (user == null)
        {
            model.ErrorMessage = "User not found or not logged in.";
            return View(model);
        }

        switch (action)
        {
            case "UpdateUsername":
                if (userService.UpdateUserUsername(model.Username, model.CurrentPassword))
                    model.SuccessMessage = "Username updated successfully!";
                else
                    model.ErrorMessage = "Failed to update username. Check your password.";
                break;
            case "UpdateEmail":
                if (userService.UpdateUserEmail(model.Email, model.CurrentPassword))
                    model.SuccessMessage = "Email updated successfully!";
                else
                    model.ErrorMessage = "Failed to update email. Check your password.";
                break;
            case "UpdatePassword":
                if (userService.UpdateUserPassword(model.Password, model.CurrentPassword))
                    model.SuccessMessage = "Password updated successfully!";
                else
                    model.ErrorMessage = "Failed to update password. Check your current password.";
                break;
            case "DeleteAccount":
                userService.DeleteUser(user.UserId);
                userService.Logout();
                return RedirectToAction("Index", "Home");
            case "Logout":
                userService.Logout();
                return RedirectToAction("Index", "Home");
        }

        // Reload user info for display
        var updatedUser = userService.GetCurrentUser();
        if (updatedUser != null)
        {
            model.Username = updatedUser.Username;
            model.Email = updatedUser.Email;
        }
        model.Password = "";
        model.CurrentPassword = "";
        return View(model);
    }

    public IActionResult ModifyProfile()
    {
        var user = userService.GetCurrentUser();
        var model = new ModifyProfileViewModel
        {
            Description = "",
            ProfilePictureUrl = user?.ProfilePicturePath
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult ModifyProfile(ModifyProfileViewModel model)
    {
        var user = userService.GetCurrentUser();
        if (user == null)
        {
            model.ErrorMessage = "User not found or not logged in.";
            return View(model);
        }

        // Handle profile picture upload
        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ProfilePicture.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                model.ProfilePicture.CopyTo(stream);
            }

            user.ProfilePicturePath = $"/uploads/{fileName}";
        }

        // Update description/bio if provided
        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            // If you have a UserProfile entity, update its Description property here
            // Otherwise, you may need to add a Description property to your User entity
            // Example: user.Description = model.Description;
        }

        // Save changes
        userService.UpdateUser(user);

        model.SuccessMessage = "Profile updated successfully!";
        model.ProfilePictureUrl = user.ProfilePicturePath;
        return View(model);
    }
}
