using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ExcursionHelperDBEditor.Context;
using ExcursionHelperDBEditor.Models;

namespace ExcursionHelperDBEditor.Views;

public partial class MainWindow : Window
{
  private PostgresContext dbcontext = new();
  public MainWindow()
  {
    InitializeComponent();
    LoadExcursionTitlesInCombobox();
    
    CommentsDateSortComboBox.SelectionChanged += ComboBoxesSelectionChanged;
    CommentsExcursionSelectionComboBox.SelectionChanged += ComboBoxesSelectionChanged;
  }

  private void LoadComments()
  {
    List<Comment> comments = dbcontext.Comments.ToList();

    if (CommentsExcursionSelectionComboBox.SelectedIndex != 0)
    {
      Excursion selectedExcursion = (Excursion) CommentsExcursionSelectionComboBox.SelectedItem;
      comments = comments.Where(c => c.ExcursionId == selectedExcursion.ExcursionId).ToList();
    }

    switch (CommentsDateSortComboBox.SelectedIndex)
    {
      case 1:
        comments = comments.OrderByDescending(c => c.CommentDate).ToList();
        break;
        
      case 2:
        comments = comments.OrderBy(c => c.CommentDate).ToList();
        break;
    }
    
    CommentsListBox.ItemsSource = comments;
  }
  
  private void ComboBoxesSelectionChanged(object? sender, SelectionChangedEventArgs e)
  {
    LoadComments();
  }

  private void LoadExcursionTitlesInCombobox()
  {
    List<Excursion> excursions = dbcontext.Excursions.OrderBy(e => e.ExcursionId).ToList();
    excursions.Insert(0, new Excursion()
    {
      ExcursionId = 0,
      Title = "Экскурсии"
    });
    CommentsExcursionSelectionComboBox.ItemsSource = excursions;
    CommentsExcursionSelectionComboBox.SelectedIndex = 0;
  }

  private void LoadExcursions()
  {
    List<Excursion> excursions = dbcontext.Excursions.OrderBy(e => e.ExcursionId).ToList();
    ExcursionsListBox.ItemsSource = excursions;
  }


  private void ExcursionMenuClick(object? sender, RoutedEventArgs e)
  {
    CommentsGrid.IsVisible = false;
    
    ExcursionsGrid.IsVisible = true;
    LoadExcursions();
  }

  private void CommentMenuClick(object? sender, RoutedEventArgs e)
  {
    ExcursionsGrid.IsVisible = false;

    CommentsGrid.IsVisible = true;
    LoadComments();
  }

  private void ExcursionsListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
  {
    var listboxObject = ExcursionsListBox.SelectedItem;
    if (listboxObject != null)
    {
      System.Reflection.PropertyInfo pi = listboxObject.GetType().GetProperty("ExcursionId");
      int id = (int)(pi.GetValue(listboxObject, null));
      ExcursionEditingWindow eew = new(id);
      eew.ShowDialog(this);
      eew.Closed += (o, arg) =>
      {
        dbcontext = new();
        LoadExcursions();
      };
    }
    ExcursionsListBox.UnselectAll();
  }

  private async void CreateNewExcursionClick(object? sender, RoutedEventArgs e)
  {
    ExcursionEditingWindow eew = new();
    await eew.ShowDialog(this);
    LoadExcursions();
  }
}