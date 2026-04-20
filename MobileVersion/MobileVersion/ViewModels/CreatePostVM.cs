using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MobileVersion.Dtos;
using MobileVersion.Messages;
using MobileVersion.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace MobileVersion.ViewModels
{
    public partial class CreatePostVM : ViewModelBase
    {
        private readonly consoleClientModel _model;

        public CreatePostVM(consoleClientModel model)
        {
            _model = model;
            InitializeCats();
        }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public ObservableCollection<CategoryDTO> Cats { get; set; } = new();
        [ObservableProperty] private CategoryDTO _selectedCategory;
        [ObservableProperty] private string? _selectedImageBase64;

        [ObservableProperty] private Bitmap? _selectedImageBitmap; // For preview display

        [ObservableProperty] private bool _hasImage = false;

        private string? _fileExtension;

        [RelayCommand]
        private void Close() => WeakReferenceMessenger.Default.Send(new GoBackMessage());

        private async Task InitializeCats()
        {
            Cats.Clear();
            var loadcats = await _model.getallcats();
            loadcats?.ForEach(r =>
            {
                if (r.categoryname != "All")
                {
                    Cats.Add(r);
                }
            });
            SelectedCategory = Cats.FirstOrDefault();
        }
        [RelayCommand]
        private void RemoveImage()
        {
            SelectedImageBase64 = null;
            _selectedImageBitmap = null;
            _fileExtension = null;
            HasImage = false;
    
            System.Diagnostics.Debug.WriteLine("Image removed");
        }
        [RelayCommand]
        private async Task BrowseImage(object? parameter)
        {
            try
            {
                // Get TopLevel from parameter
                TopLevel? topLevel = parameter as TopLevel;
                if (topLevel?.StorageProvider == null)
                {
                    System.Diagnostics.Debug.WriteLine("StorageProvider not available");
                    return;
                }

                // Configure file picker
                var options = new FilePickerOpenOptions
                {
                    Title = "Select an image",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Images")
                        {
                            Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.webp" },
                            MimeTypes = new[] { "image/*" }
                        }
                    }
                };

                // Open file picker
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);

                if (files.Count == 0) return; // User cancelled

                var file = files[0];
                
                // Read file as bytes using StorageFile stream (works on Android content URIs)
                await using var stream = await file.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                
                // For display path, use file name instead of full path
                var filePath = file.Name;

                // Validate file size (max 5MB)
                if (imageBytes.Length > 5 * 1024 * 1024)
                {
                    System.Diagnostics.Debug.WriteLine("File size exceeds 5MB limit");
                    // TODO: Show error message to user
                    return;
                }

                // Convert to Base64
                SelectedImageBase64 = Convert.ToBase64String(imageBytes);

                // Store path for preview
                SelectedImageBitmap = new Bitmap(new MemoryStream(imageBytes));

                // Store extension
                _fileExtension = Path.GetExtension(filePath);

                // Set flag
                HasImage = true;

                System.Diagnostics.Debug.WriteLine($"Image selected: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting image: {ex.Message}");
                // Reset state on error
                RemoveImage();
            }
        }

        [RelayCommand]
        private async Task CreatePost()
        {
            if (SelectedCategory != null && _model.CurrentUser != null)
            {
                try
                {
                    await _model.createPost(new PostDTO
                    {
                        userID = _model.CurrentUser.UserID,
                        categoryname = SelectedCategory.categoryname,
                        title = Title,
                        content = Content,
                        created_at = DateTime.Now,
                        ImageBase64 = HasImage ? SelectedImageBase64 : null,
                        FileExtension = HasImage ? _fileExtension : null
                    });
                    WeakReferenceMessenger.Default.Send(new GoBackMessage());
                    WeakReferenceMessenger.Default.Send(new PostCreatedMessage());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating post: {ex.Message}");
                }
            }
        }
    }
}