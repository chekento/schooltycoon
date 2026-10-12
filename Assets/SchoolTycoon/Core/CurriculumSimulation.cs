using System;
using System.Collections.Generic;
using System.Linq;
namespace KoSch.SchoolTycoon.Core
{
    public sealed partial class SchoolSimulation
    {
        public int Weekday { get { return (State.Day-1)%5; } }
        public List<Subject> Today(SchoolClass c) { return c.Week.Skip(Weekday*6).Take(6).ToList(); }
        public IEnumerable<Subject> PupilCourses(Pupil p) { SchoolClass c=PupilClass(p);return c==null ? State.Timetable.Distinct() : c.Week.Distinct(); }
        public int Score(Pupil p,Subject sub)
        {
            if((int)sub<5) return p.Scores[(int)sub];
            CourseMark m=p.CourseMarks.FirstOrDefault(x=>x.Subject==sub);return m!=null ? m.Score : p.Scores[(int)CurriculumCatalog.Foundation(CurriculumCatalog.Get(sub,State).Group)];
        }
        public void SetScore(Pupil p, Subject sub,int value)
        {
            value=Clamp(value,0,100);if((int)sub<5){p.Scores[(int)sub]=value;return;}
            CourseMark m=p.CourseMarks.FirstOrDefault(x=>x.Subject==sub);if(m==null){m=new CourseMark{Subject=sub};p.CourseMarks.Add(m);}m.Score=value;
        }
        private void EnsureCourseMarks()
        { foreach(Pupil p in State.Pupils) foreach(Subject sub in PupilCourses(p).Where(s=>(int)s>=5).ToList()) if(!p.CourseMarks.Any(m=>m.Subject==sub)) SetScore(p,sub,Score(p,sub)); }
        public int PupilAverage(Pupil p) { return (int)PupilCourses(p).Average(s=>Score(p,s)); }
        public int WeakestScore(Pupil p) { return PupilCourses(p).Min(s=>Score(p,s)); }
        public bool NeedsSupport(Pupil p) { return WeakestScore(p)<45 || p.Stress>65 || p.Wellbeing<40; }
        public bool Expertise(Employee e, Subject sub)
        { return e!=null && ((int)e.Specialty<5 ? e.Specialty==CurriculumCatalog.Foundation(CurriculumCatalog.Get(sub,State).Group) : CurriculumCatalog.Get(e.Specialty,State).Group==CurriculumCatalog.Get(sub,State).Group); }
        public Result SetWeeklyLesson(int room,int day,int slot,Subject sub)
        {
            if(!CanOperate)return Result.Fail("blocked");SchoolClass c=State.Classes.FirstOrDefault(g=>g.RoomId==room);
            if(c==null||day<0||day>4||slot<0||slot>5||!CurriculumCatalog.Valid(sub,State))return Result.Fail("invalid");
            c.Week[day*6+slot]=sub;if(day==0)c.Lessons[slot]=sub;EnsureCourseMarks();return Result.Ok();
        }
        public Result ApplyCurriculum(CurriculumRegion region,int room=0)
        {
            if(!CanOperate)return Result.Fail("blocked");if(!Enum.IsDefined(typeof(CurriculumRegion),region)||room!=0&&!State.Classes.Any(c=>c.RoomId==room))return Result.Fail("invalid");
            foreach(SchoolClass c in State.Classes.Where(c=>room==0||c.RoomId==room)){c.Week=CurriculumCatalog.Week(region);c.Lessons=c.Week.Take(6).ToList();}
            if(room==0){State.Region=region;State.Timetable=CurriculumCatalog.Week(region).Take(6).ToList();}EnsureCourseMarks();return Result.Ok();
        }
        public Result SetCourseLevel(int room,int level)
        { if(!CanOperate)return Result.Fail("blocked");SchoolClass c=State.Classes.FirstOrDefault(g=>g.RoomId==room);if(c==null||level<1||level>3)return Result.Fail("invalid");c.CourseLevel=level;return Result.Ok(); }
        public Result SetSchoolPath(int grade)
        { if(!CanOperate)return Result.Fail("blocked");if(grade<6||grade>13||State.Pupils.Any(p=>p.Grade>grade))return Result.Fail("grade");State.GraduationGrade=grade;return Result.Ok(); }
        public Result AddSubject(string de,string en,SubjectGroup group,RoomKind facility,int difficulty)
        {
            if(!CanOperate)return Result.Fail("blocked");if(string.IsNullOrWhiteSpace(de)||string.IsNullOrWhiteSpace(en)||de.Trim().Length>60||en.Trim().Length>60||!Enum.IsDefined(typeof(SubjectGroup),group)||!Enum.IsDefined(typeof(RoomKind),facility)||facility==RoomKind.Corridor||facility==RoomKind.Staircase||difficulty<1||difficulty>3||State.CustomSubjects.Count>=200)return Result.Fail("invalid");
            if(CurriculumCatalog.All(State).Any(c=>string.Equals(c.NameDe,de.Trim(),StringComparison.OrdinalIgnoreCase)||string.Equals(c.NameEn,en.Trim(),StringComparison.OrdinalIgnoreCase)))return Result.Fail("done");
            State.CustomSubjects.Add(new SubjectSpec((Subject)State.NextCustomSubject++,de.Trim(),en.Trim(),group,facility,difficulty,7,difficulty-1));return Result.Ok();
        }
        public int BuildCost(RoomKind kind,int floor=0) { return Catalog.Get(kind).Cost*(100+floor*20)/100; }
        public int RoomRunningCost(Room r) { return (Catalog.Get(r.Kind).DailyCost*(r.Kind==RoomKind.Corridor?r.Width*r.Height:1)+r.UpgradeLevel*8)*(100+r.Floor*10)/100; }
        public int TeachingCost() { return State.Classes.Where(c=>ClassTeacher(c)!=null).Sum(c=>ClassSize(c)*(c.CourseLevel-1+Today(c).Sum(s=>CurriculumCatalog.Get(s,State).Materials))); }
        public int FacilitySeats(Room r) { return (Catalog.Get(r.Kind).Capacity>0 ? Catalog.Get(r.Kind).Capacity:24)+r.UpgradeLevel*4; }
        public Room SpecialistRoom(SchoolClass requested,Subject subject,int slot)
        {
            RoomKind kind=CurriculumCatalog.Get(subject,State).Facility;if(kind==RoomKind.Classroom||requested==null)return null;
            var used=new HashSet<int>();
            foreach(SchoolClass c in State.Classes.Where(c=>ClassTeacher(c)!=null&&CurriculumCatalog.Get(Today(c)[slot],State).Facility==kind).OrderBy(c=>c.RoomId))
            {
                Room home=State.Rooms.First(r=>r.Id==c.RoomId);
                Room choice=State.Rooms.Where(r=>r.Kind==kind&&!used.Contains(r.Id)&&FacilitySeats(r)>=ClassSize(c)).OrderBy(r=>Math.Abs(r.Floor-home.Floor)).ThenBy(r=>r.Id).FirstOrDefault();
                if(choice!=null)used.Add(choice.Id);if(c==requested)return choice;
            }
            return null;
        }
        public int LessonFacilityBonus(SchoolClass c,Subject sub)
        {
            int library=Math.Min(8,State.Rooms.Where(r=>r.Kind==RoomKind.Library).Sum(r=>3+r.UpgradeLevel));
            int slot=Today(c).IndexOf(sub);Room r=slot<0?null:SpecialistRoom(c,sub,slot);return library+(r==null?0:5+r.UpgradeLevel*2);
        }
        private void ClubLearning(Pupil p)
        {
            if(p.Absent)return;int added=0;
            foreach(ClubKind k in State.Clubs.Where(ClubAvailable).OrderBy(k=>k))
            {
                if(added>=2)break;Subject foundation=AcademyCatalog.Clubs[(int)k].Subject;
                var eligible=PupilCourses(p).Where(s=>Researched(ResearchKind.CreativeCampus)||CurriculumCatalog.Foundation(CurriculumCatalog.Get(s,State).Group)==foundation).ToList();
                if(eligible.Count==0)eligible.Add(foundation);Subject sub=eligible[(p.Id+State.Day+(int)k)%eligible.Count];SetScore(p,sub,Score(p,sub)+1);added++;
            }
        }
        public static readonly string[] CareerDe={"Breite Bildung","Fachcampus","Schule in den Wolken","Starke Gemeinschaft"};
        public static readonly string[] CareerEn={"Broad education","Specialist campus","School in the sky","Strong community"};
        public IEnumerable<int> CareerChoices() { return Enumerable.Range(0,3).Select(i=>(SchoolYear-1+i)%4); }
        public int CareerCost { get { return Math.Min(2400,400+SchoolYear*100); } }
        public int CareerReward { get { return Math.Min(5000,2000+State.Prestige*150); } }
        public Result StartCareer(int kind)
        {
            if(!CanOperate)return Result.Fail("blocked");if(State.Level<1)return Result.Fail("locked");if(State.ActiveContract>=0)return Result.Fail("busy");if(State.ContractYear==SchoolYear)return Result.Fail("done");if(YearDay>51)return Result.Fail("lateyear");if(!CareerChoices().Contains(kind))return Result.Fail("invalid");if(State.Cash<CareerCost)return Result.Fail("funds");
            State.Cash-=CareerCost;State.ActiveContract=kind;State.ContractYear=SchoolYear;State.ContractProgress=0;return Result.Ok();
        }
        public bool CareerCondition(int kind)
        {
            Ledger f=Forecast();if(State.Cash<0||State.Happiness<60||State.GameOver||f.Income<f.Expenses)return false;
            switch(kind)
            {
                case 0:return State.Classes.SelectMany(c=>c.Week).Select(s=>CurriculumCatalog.Get(s,State).Group).Distinct().Count()>=7&&State.Learning>=65;
                case 1:return State.Staff.Count(e=>e.Role==StaffRole.Teacher&&e.TrainingLevel>0)>=3&&State.Rooms.Where(r=>(int)r.Kind>=7&&r.Kind!=RoomKind.Staircase).Select(r=>r.Kind).Distinct().Count()>=4;
                case 2:return State.Classes.Any(c=>State.Rooms.First(r=>r.Id==c.RoomId).Floor>=2)&&State.Classes.Count>=4&&State.Students>=72;
                default:return State.Students>=48&&State.Happiness>=80&&State.Cleanliness>=80&&State.SupportPolicy>0;
            }
        }
        private void RunCareer()
        {
            if(State.ActiveContract<0)return;if(!CareerCondition(State.ActiveContract)){State.ContractProgress=0;return;}
            if(++State.ContractProgress<10)return;int reward=CareerReward;
            State.DailyReport.CareerFinished=State.ActiveContract;State.DailyReport.CareerReward=reward;
            State.CareerHistory.Add(new CareerRecord{Day=State.Day,Year=SchoolYear,Kind=State.ActiveContract,Reward=reward});if(State.CareerHistory.Count>60)State.CareerHistory.RemoveAt(0);
            State.Cash+=reward;State.Prestige++;State.ActiveContract=-1;State.ContractProgress=0;
        }
        private static bool ValidateCurriculum(SchoolState s)
        {
            if(s.CustomSubjects==null||s.CareerHistory==null||s.GraduationGrade<6||s.GraduationGrade>13||!Enum.IsDefined(typeof(CurriculumRegion),s.Region)||s.CustomSubjects.Count>200||s.NextCustomSubject!=1000+s.CustomSubjects.Count)return false;
            for(int i=0;i<s.CustomSubjects.Count;i++){SubjectSpec c=s.CustomSubjects[i];if(c==null||(int)c.Id!=1000+i||string.IsNullOrWhiteSpace(c.NameDe)||string.IsNullOrWhiteSpace(c.NameEn)||c.NameDe.Length>60||c.NameEn.Length>60||!Enum.IsDefined(typeof(SubjectGroup),c.Group)||!Enum.IsDefined(typeof(RoomKind),c.Facility)||c.Facility==RoomKind.Corridor||c.Facility==RoomKind.Staircase||c.Difficulty<1||c.Difficulty>3||c.Materials<0||c.Materials>2||c.Regions!=7)return false;}
            if(s.Classes.Any(c=>c.Week==null||c.Week.Count!=30||c.Week.Any(t=>!CurriculumCatalog.Valid(t,s))||c.CourseLevel<1||c.CourseLevel>3))return false;
            foreach(Pupil p in s.Pupils)if(p.CourseMarks==null||p.CourseMarks.Count>346||p.CourseMarks.Any(m=>m==null||(int)m.Subject<5||!CurriculumCatalog.Valid(m.Subject,s)||m.Score<0||m.Score>100||m.Lessons<0||m.LastTaught<0||m.LastTaught>=s.Day)||p.CourseMarks.Select(m=>m.Subject).Distinct().Count()!=p.CourseMarks.Count)return false;
            if(s.Reports.Any(t=>t.Courses==null||t.Courses.Any(c=>c==null||!CurriculumCatalog.Valid(c.Subject,s)||c.Average<0||c.Average>100||c.Students<0)))return false;
            if(s.ActiveContract<-1||s.ActiveContract>3||s.ContractYear<0||s.ContractYear>(s.Day-1)/60+1||s.ContractProgress<0||s.ContractProgress>=10||s.ActiveContract==-1&&s.ContractProgress!=0||s.Prestige<0||s.CareerHistory.Count>60||s.CareerHistory.Any(c=>c==null||c.Day<1||c.Day>=s.Day||c.Year!=(c.Day-1)/60+1||c.Kind<0||c.Kind>3||c.Reward<2000||c.Reward>5000)||s.CareerHistory.Select(c=>c.Year).Distinct().Count()!=s.CareerHistory.Count)return false;
            return s.DailyReport.CareerFinished>=-1&&s.DailyReport.CareerFinished<=3&&s.DailyReport.CareerReward>=0;
        }
    }
}
