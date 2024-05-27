using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using ExcursionHelperDBEditor.Context;
using ExcursionHelperDBEditor.Models;
using Image = ExcursionHelperDBEditor.Models.Image;

namespace ExcursionHelperDBEditor.Views;

public partial class ExcursionEditingWindow : Window
{
  private PostgresContext dbcontext = new();
  private Excursion _excursion;
  private bool newExcursion;
  private bool imageDataChanged = false;
  private bool imagesError = false;
  private Checkpoint _selectedCheckpoint;
  
  public ExcursionEditingWindow()
  {
    InitializeComponent();
    _excursion = new();
    newExcursion = true;
  }

  public ExcursionEditingWindow(int id)
  {
    InitializeComponent();
    _excursion = dbcontext.Excursions.Where(e => e.ExcursionId == id).FirstOrDefault();
    ExcursionTitleTextBox.Text = _excursion.Title;
    ExcursionDescriptionTextBox.Text = _excursion.Description;
    ExcursionMapImageUrlTextBox.Text = _excursion.MapImageUrl;
    ExcursionMapUrlTextBox.Text = _excursion.MapUrl;
    List<Checkpoint> checkpoints = _excursion.Checkpoints.ToList();
    CheckpointsListbox.ItemsSource = checkpoints;
  }

  private void SaveButtonClick(object? sender, RoutedEventArgs e)
  {
    bool thereAnError = false;
    if (ExcursionTitleTextBox.Text is null)
    {
      ExcursionTitleTextBox.Background = Brushes.Red;
      thereAnError = true;
      ExcursionTitleTextBox.KeyUp += ExcursionTitleTextBox_OnKeyUp;
    }
    if (ExcursionDescriptionTextBox.Text is null)
    {
      ExcursionDescriptionTextBox.Background = Brushes.Red;
      thereAnError = true;
      ExcursionDescriptionTextBox.KeyUp += ExcursionDescriptionTextBox_OnKeyUp;
    }
    if (ExcursionMapUrlTextBox.Text is null)
    {
      ExcursionMapUrlTextBox.Background = Brushes.Red;
      thereAnError = true;
      ExcursionMapUrlTextBox.KeyUp += ExcursionMapUrlTextBox_OnKeyUp;
    }

    List<Checkpoint> checkpoints = new();
    checkpoints = CheckpointsListbox.Items.Cast<Checkpoint>().OrderBy(c => c.OrderNumber).ToList();
    for (int i = 0; i < checkpoints.Count; i++)
    {
      if (checkpoints[i].OrderNumber != i+1 || checkpoints[i].Title == null)
      { 
        CheckpointsListbox.Background = Brushes.Red; 
        thereAnError = true; 
        break;
      }
    }

    if (!imagesError && ImagesListbox.ItemsSource != null)
    {
      List<Image> images = ImagesListbox.ItemsSource.Cast<Image>().ToList();
      foreach (var image in images)
      {
        if (image.ImageUrl == null || image.ImageDescription == null)
        {
          ImagesListbox.Background = Brushes.Red;
          CheckpointsListbox.SelectedItem = _selectedCheckpoint;
          imagesError = true;
        }
      }
    }
    
    
    if (!thereAnError && !imagesError)
    {
      _excursion.Title = ExcursionTitleTextBox.Text;
      _excursion.Description = ExcursionDescriptionTextBox.Text;
      _excursion.MapImageUrl = ExcursionMapImageUrlTextBox.Text;
      _excursion.MapUrl = ExcursionMapUrlTextBox.Text;
      _excursion.Checkpoints = CheckpointsListbox.ItemsSource.Cast<Checkpoint>().ToList();
      if (newExcursion)
      {
        dbcontext.Excursions.Add(_excursion);
      }
      else
      {
        dbcontext.Excursions.Update(_excursion);
      }
      dbcontext.Dbversions.Add(new Dbversion());
      dbcontext.SaveChanges();
      Close();
    }
  }

  private void CheckpointsListbox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
  {
    if (CheckpointsListbox.SelectedItem != null && CheckpointsListbox.SelectedItem != _selectedCheckpoint)
    {
      if (imageDataChanged)
      {
        List<Image> images = ImagesListbox.ItemsSource.Cast<Image>().ToList();
        foreach (var image in images)
        {
          if (image.ImageUrl == null || image.ImageDescription == null)
          {
            ImagesListbox.Background = Brushes.Red;
            CheckpointsListbox.SelectedItem = _selectedCheckpoint;
            imagesError = true;
            return;
          }
        }
        _selectedCheckpoint.Images = images;
        dbcontext.Checkpoints.Update(_selectedCheckpoint);
        imagesError = false;
        ImagesListbox.Background = Brushes.WhiteSmoke;
      }
      _selectedCheckpoint = (Checkpoint) CheckpointsListbox.SelectedItem;
      ImagesListbox.ItemsSource = _selectedCheckpoint.Images.ToList();
      imageDataChanged = false;
    }
    CheckpointsListbox.Background = Brushes.WhiteSmoke;
  }

  private void AddImage(object? sender, RoutedEventArgs e)
  {
    if (CheckpointsListbox.SelectedItem != null)
    {
      var images = ImagesListbox.ItemsSource.Cast<Image>().ToList();
      images.Add(new Image());
      ImagesListbox.ItemsSource = images;
    }
  }
  
  private async void AddExistingImage(object? sender, RoutedEventArgs e)
  {
    if (CheckpointsListbox.SelectedItem != null)
    {
      var images = ImagesListbox.ItemsSource.Cast<Image>().ToList();
      SelectImageWindow SIW = new();
      Image addImage = await SIW.ShowDialog<Image>(this);
      if (addImage != null)
      {
        images.Add(addImage);
        ImagesListbox.ItemsSource = images;
        imageDataChanged = true;
      }
    }
  }
  
  private void RemoveImage(object? sender, RoutedEventArgs e)
  {
    if (ImagesListbox.SelectedItem != null  || CheckpointsListbox.SelectedItem != null)
    {
      var images = ImagesListbox.ItemsSource.Cast<Image>().ToList();
      Image imageToDelete = (Image)ImagesListbox.SelectedItem;
      Checkpoint selectedCheckpoint = (Checkpoint) CheckpointsListbox.SelectedItem;
      selectedCheckpoint.Images.Remove(imageToDelete);
      images.Remove(imageToDelete);
      ImagesListbox.ItemsSource = images;
    }
  }

  private void AddCheckpoint(object? sender, RoutedEventArgs e)
  {
    if (CheckpointsListbox.ItemsSource != null)
    {
      var checkpoints = CheckpointsListbox.ItemsSource.Cast<Checkpoint>().OrderBy(c => c.OrderNumber).ToList();
      checkpoints.Add(new Checkpoint()
        {
          OrderNumber = Convert.ToInt16(checkpoints.Count+1) 
        }
      );
      CheckpointsListbox.ItemsSource = checkpoints;
      return;
    }

    List<Checkpoint> emptyCheckpoint = new List<Checkpoint>();
    emptyCheckpoint.Add(new Checkpoint()
    {
      OrderNumber = 1
    });
    CheckpointsListbox.ItemsSource = emptyCheckpoint;
  }
  
  private void RemoveCheckpoint(object? sender, RoutedEventArgs e)
  {
    if (CheckpointsListbox.SelectedItem != null)
    {
      var checkpoints = CheckpointsListbox.ItemsSource.Cast<Checkpoint>().ToList();
      Checkpoint checkpointToDelete = (Checkpoint) CheckpointsListbox.SelectedItem;
      if (checkpointToDelete.CheckpointId == 0)
      {
        checkpoints.Remove(checkpointToDelete);
        CheckpointsListbox.ItemsSource = checkpoints;
        return;
      }

      checkpointToDelete.Images.Clear();
      checkpointToDelete.Excursions.Remove(_excursion);
      checkpoints.Remove(checkpointToDelete);
      dbcontext.Checkpoints.Remove(checkpointToDelete);
      CheckpointsListbox.ItemsSource = checkpoints;
      ImagesListbox.ItemsSource = new List<Image>();
    }
  }
  
  private void ImageTextChanged(object? sender, KeyEventArgs e)
  {
    imageDataChanged = true;
    if (imagesError)
    {
      ImagesListbox.Background = Brushes.WhiteSmoke;
    }
  }

  private void ExcursionTitleTextBox_OnKeyUp(object? sender, KeyEventArgs e)
  {
    ExcursionTitleTextBox.Background = Brushes.White;
    ExcursionTitleTextBox.KeyUp += null;
  }

  private void ExcursionMapUrlTextBox_OnKeyUp(object? sender, KeyEventArgs e)
  {
    ExcursionMapUrlTextBox.Background = Brushes.White;
    ExcursionMapUrlTextBox.KeyUp += null;
  }

  private void ExcursionDescriptionTextBox_OnKeyUp(object? sender, KeyEventArgs e)
  {
    ExcursionDescriptionTextBox.Background = Brushes.White;
    ExcursionDescriptionTextBox.KeyUp += null;
  }

  private void ExithWithoutSaving(object? sender, RoutedEventArgs e)
  {
    Close();
  }

  private void DeleteExcursion(object? sender, RoutedEventArgs e)
  {
    if (newExcursion)
    {
      Close();
      return;
    }

    _excursion.Checkpoints = new List<Checkpoint>();
    dbcontext.Excursions.Remove(_excursion);
    dbcontext.Dbversions.Add(new Dbversion());
    dbcontext.SaveChanges();
    Close();
  }
}