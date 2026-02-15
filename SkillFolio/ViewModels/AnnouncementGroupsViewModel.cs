using SkillFolio.Models;

namespace SkillFolio.ViewModels
{
    public class AnnouncementGroupsViewModel
    {
        public bool HasSchoolGroup { get; set; }
        public bool HasDepartmentGroup { get; set; }

        public List<Announcement> SchoolAnnouncements { get; set; } = new();
        public List<Announcement> DepartmentAnnouncements { get; set; } = new();
    }
}
