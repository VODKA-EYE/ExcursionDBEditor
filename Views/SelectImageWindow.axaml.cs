using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ExcursionHelperDBEditor.Context;
using Image = ExcursionHelperDBEditor.Models.Image;

namespace ExcursionHelperDBEditor.Views;

public partial class SelectImageWindow : Window
{
  private PostgresContext dbcontext = new();
  public SelectImageWindow()
  {
    InitializeComponent();
    LoadImages();
  }

  private void LoadImages()
  {
    List<Image> images = dbcontext.Images.ToList();

    // Search bar
    string searchString = SearchTextBox.Text ?? "";
    searchString = searchString.ToLower();
    string[] searchStringElements = searchString.Split(' ');

    if (!string.IsNullOrEmpty(searchString))
    {
      foreach (var element in searchStringElements)
      {
        images = images.Where(c =>
          c.ImageUrl.ToLower().Contains(element) ||
          c.ImageDescription.ToLower().Contains(element)
        ).ToList();
      }
    }
    
    ImagesListBox.ItemsSource = images;
  }

  private void ImagesListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
  {
    Image selectedImage = (Image) ImagesListBox.SelectedItem;
    Close(selectedImage);
  }

  private void SearchTextBox_OnKeyUp(object? sender, KeyEventArgs e)
  {
    LoadImages();
  }
  
  private void CancelClick(object? sender, RoutedEventArgs e)
  {
    Close();
  }
}