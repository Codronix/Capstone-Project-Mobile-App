using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms.Maps;

namespace Kerlyn_Liner_App.DataProperties
{
    class Location_Data : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Position _position;
        public string Description { get; }

        public Position Position
        {
            get => _position;
            set
            {
                if (!_position.Equals(value))
                {
                    _position = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
                }
            }
        }
        public Location_Data(string description, Position position)
        {
            Description = description;
            Position = position;
        }
    }
}