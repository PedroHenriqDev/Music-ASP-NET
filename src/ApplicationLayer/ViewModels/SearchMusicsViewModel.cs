﻿using DomainLayer.Entities;

namespace ApplicationLayer.ViewModels;

public class SearchMusicsViewModel
{
    public Listener Listener { get; set; }
    public IEnumerable<MusicViewModel>? MusicsSuggestion { get; set; }
    public IEnumerable<MusicViewModel> FoundMusics { get; set; }
    public string? MusicsToAdd {  get; set; }
}
