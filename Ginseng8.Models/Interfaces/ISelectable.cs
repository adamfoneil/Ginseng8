﻿namespace Ginseng.Models.Interfaces
{
    public interface ISelectable
    {
        int Id { get; set; }
        string Name { get; set; }
        bool Selected { get; set; }
        string BackColor { get; set; }
        string ForeColor { get; set; }
    }
}