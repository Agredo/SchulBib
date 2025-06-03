using SchulBib.Models.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchulBib.Models.Entities;

public class AppSetting : BaseEntity
{
    [Required, MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public SettingCategory Category { get; set; } = SettingCategory.General;

    public bool IsSystemSetting { get; set; } = false; // System vs. User Settings
}