using System;
using System.Collections.Generic;
using System.Linq;
namespace KoSch.SchoolTycoon.Core
{
    public enum SubjectGroup { Mathematics, Languages, Science, Humanities, SocialStudies, CreativeArts, Computing, Technology, Health, Sports }
    public enum CurriculumRegion { International, Germany, England, USA }
    [Serializable] public sealed class SubjectSpec
    {
        public Subject Id; public string NameDe, NameEn;
        public SubjectGroup Group; public RoomKind Facility;
        public int Difficulty, Regions, Materials;
        public SubjectSpec() { }
        public SubjectSpec(Subject id, string de, string en, SubjectGroup group, RoomKind facility, int difficulty, int regions, int materials)
        { Id=id; NameDe=de; NameEn=en; Group=group; Facility=facility; Difficulty=difficulty; Regions=regions; Materials=materials; }
    }
    [Serializable] public sealed class CourseMark { public Subject Subject; public int Score, Lessons, LastTaught; }
    [Serializable] public sealed class CourseResult { public Subject Subject; public int Average, Students; }
    [Serializable] public sealed class CareerRecord { public int Day, Year, Kind, Reward; }
    public static class CurriculumCatalog
    {
        public static readonly SubjectSpec[] Subjects = {
            new SubjectSpec(Subject.Mathematics, "Mathematik", "Mathematics", SubjectGroup.Mathematics, RoomKind.Classroom, 1, 7, 0),
            new SubjectSpec(Subject.Languages, "Sprachen", "Languages", SubjectGroup.Languages, RoomKind.LanguageLab, 1, 7, 0),
            new SubjectSpec(Subject.Science, "Naturwissenschaft", "Science", SubjectGroup.Science, RoomKind.ScienceLab, 1, 7, 0),
            new SubjectSpec(Subject.Arts, "Kunst", "Arts", SubjectGroup.CreativeArts, RoomKind.ArtRoom, 1, 7, 0),
            new SubjectSpec(Subject.Sports, "Sport", "Sports", SubjectGroup.Sports, RoomKind.Gym, 1, 7, 0),
            new SubjectSpec(Subject.Algebra, "Algebra", "Algebra", SubjectGroup.Mathematics, RoomKind.Classroom, 2, 7, 0),
            new SubjectSpec(Subject.Geometry, "Geometrie", "Geometry", SubjectGroup.Mathematics, RoomKind.Classroom, 2, 7, 0),
            new SubjectSpec(Subject.Trigonometry, "Trigonometrie", "Trigonometry", SubjectGroup.Mathematics, RoomKind.Classroom, 2, 7, 0),
            new SubjectSpec(Subject.Precalculus, "Analysis-Grundlagen", "Precalculus", SubjectGroup.Mathematics, RoomKind.Classroom, 2, 7, 0),
            new SubjectSpec(Subject.Calculus, "Analysis", "Calculus", SubjectGroup.Mathematics, RoomKind.Classroom, 3, 7, 2),
            new SubjectSpec(Subject.Statistics, "Statistik", "Statistics", SubjectGroup.Mathematics, RoomKind.ComputerLab, 2, 7, 0),
            new SubjectSpec(Subject.FurtherMathematics, "Vertiefte Mathematik", "Further Mathematics", SubjectGroup.Mathematics, RoomKind.Classroom, 3, 7, 2),
            new SubjectSpec(Subject.APCalculusAB, "AP Calculus AB", "AP Calculus AB", SubjectGroup.Mathematics, RoomKind.Classroom, 3, 4, 2),
            new SubjectSpec(Subject.APCalculusBC, "AP Calculus BC", "AP Calculus BC", SubjectGroup.Mathematics, RoomKind.Classroom, 3, 4, 2),
            new SubjectSpec(Subject.APStatistics, "AP Statistics", "AP Statistics", SubjectGroup.Mathematics, RoomKind.ComputerLab, 3, 4, 2),
            new SubjectSpec(Subject.German, "Deutsch", "German", SubjectGroup.Languages, RoomKind.LanguageLab, 1, 1, 0),
            new SubjectSpec(Subject.English, "Englisch", "English", SubjectGroup.Languages, RoomKind.LanguageLab, 1, 7, 0),
            new SubjectSpec(Subject.French, "Französisch", "French", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Spanish, "Spanisch", "Spanish", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Latin, "Latein", "Latin", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.AncientGreek, "Altgriechisch", "Ancient Greek", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Italian, "Italienisch", "Italian", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Russian, "Russisch", "Russian", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Mandarin, "Chinesisch", "Mandarin", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Japanese, "Japanisch", "Japanese", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Arabic, "Arabisch", "Arabic", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Portuguese, "Portugiesisch", "Portuguese", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Turkish, "Türkisch", "Turkish", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Polish, "Polnisch", "Polish", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Hebrew, "Hebräisch", "Hebrew", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.EnglishAdditional, "Englisch als Zweitsprache", "English Additional", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.GermanAdditional, "Deutsch als Zweitsprache", "German Additional", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 1, 0),
            new SubjectSpec(Subject.EnglishLiterature, "Englische Literatur", "English Literature", SubjectGroup.Languages, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.GermanLiterature, "Deutsche Literatur", "German Literature", SubjectGroup.Languages, RoomKind.Library, 2, 1, 0),
            new SubjectSpec(Subject.AmericanLiterature, "Amerikanische Literatur", "American Literature", SubjectGroup.Languages, RoomKind.Library, 2, 4, 0),
            new SubjectSpec(Subject.WorldLiterature, "Weltliteratur", "World Literature", SubjectGroup.Languages, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.CreativeWriting, "Kreatives Schreiben", "Creative Writing", SubjectGroup.Languages, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.Communication, "Kommunikation", "Communication", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.SignLanguage, "Gebärdensprache", "Sign Language", SubjectGroup.Languages, RoomKind.LanguageLab, 2, 7, 0),
            new SubjectSpec(Subject.Journalism, "Journalismus", "Journalism", SubjectGroup.Languages, RoomKind.ComputerLab, 2, 7, 0),
            new SubjectSpec(Subject.Biology, "Biologie", "Biology", SubjectGroup.Science, RoomKind.ScienceLab, 2, 7, 1),
            new SubjectSpec(Subject.Chemistry, "Chemie", "Chemistry", SubjectGroup.Science, RoomKind.ScienceLab, 2, 7, 1),
            new SubjectSpec(Subject.Physics, "Physik", "Physics", SubjectGroup.Science, RoomKind.ScienceLab, 2, 7, 1),
            new SubjectSpec(Subject.EarthScience, "Geowissenschaften", "Earth Science", SubjectGroup.Science, RoomKind.ScienceLab, 2, 7, 1),
            new SubjectSpec(Subject.EnvironmentalScience, "Umweltwissenschaften", "Environmental Science", SubjectGroup.Science, RoomKind.ScienceLab, 2, 7, 1),
            new SubjectSpec(Subject.Astronomy, "Astronomie", "Astronomy", SubjectGroup.Science, RoomKind.ScienceLab, 2, 7, 1),
            new SubjectSpec(Subject.Anatomy, "Anatomie", "Anatomy", SubjectGroup.Science, RoomKind.ScienceLab, 3, 7, 2),
            new SubjectSpec(Subject.APBiology, "AP Biology", "AP Biology", SubjectGroup.Science, RoomKind.ScienceLab, 3, 4, 2),
            new SubjectSpec(Subject.APChemistry, "AP Chemistry", "AP Chemistry", SubjectGroup.Science, RoomKind.ScienceLab, 3, 4, 2),
            new SubjectSpec(Subject.APPhysics1, "AP Physics1", "AP Physics1", SubjectGroup.Science, RoomKind.ScienceLab, 3, 4, 2),
            new SubjectSpec(Subject.APPhysics2, "AP Physics2", "AP Physics2", SubjectGroup.Science, RoomKind.ScienceLab, 3, 4, 2),
            new SubjectSpec(Subject.APPhysicsCMechanics, "AP Physics C: Mechanics", "AP Physics C: Mechanics", SubjectGroup.Science, RoomKind.ScienceLab, 3, 4, 2),
            new SubjectSpec(Subject.APPhysicsCElectricity, "AP Physics C: Electricity and Magnetism", "AP Physics C: Electricity and Magnetism", SubjectGroup.Science, RoomKind.ScienceLab, 3, 4, 2),
            new SubjectSpec(Subject.APEnvironmentalScience, "AP Environmental Science", "AP Environmental Science", SubjectGroup.Science, RoomKind.ScienceLab, 3, 4, 2),
            new SubjectSpec(Subject.PrimaryStudies, "Sachunterricht", "Primary Studies", SubjectGroup.Humanities, RoomKind.Classroom, 1, 1, 0),
            new SubjectSpec(Subject.History, "Geschichte", "History", SubjectGroup.Humanities, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.Geography, "Geografie", "Geography", SubjectGroup.Humanities, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.WorldHistory, "Weltgeschichte", "World History", SubjectGroup.Humanities, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.EuropeanHistory, "Europäische Geschichte", "European History", SubjectGroup.Humanities, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.USHistory, "US-Geschichte", "USHistory", SubjectGroup.Humanities, RoomKind.Library, 2, 4, 0),
            new SubjectSpec(Subject.APWorldHistory, "AP World History: Modern", "AP World History: Modern", SubjectGroup.Humanities, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APUSHistory, "AP USHistory", "AP USHistory", SubjectGroup.Humanities, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APEuropeanHistory, "AP European History", "AP European History", SubjectGroup.Humanities, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APHumanGeography, "AP Human Geography", "AP Human Geography", SubjectGroup.Humanities, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.Philosophy, "Philosophie", "Philosophy", SubjectGroup.Humanities, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.Ethics, "Ethik", "Ethics", SubjectGroup.Humanities, RoomKind.Library, 1, 7, 0),
            new SubjectSpec(Subject.ReligiousEducation, "Religionskunde", "Religious Education", SubjectGroup.Humanities, RoomKind.Library, 1, 7, 0),
            new SubjectSpec(Subject.ChristianReligion, "Christliche Religion", "Christian Religion", SubjectGroup.Humanities, RoomKind.Library, 1, 1, 0),
            new SubjectSpec(Subject.OtherReligions, "Weltreligionen", "Other Religions", SubjectGroup.Humanities, RoomKind.Library, 1, 7, 0),
            new SubjectSpec(Subject.GlobalPerspectives, "Globale Perspektiven", "Global Perspectives", SubjectGroup.Humanities, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.APAfricanAmericanStudies, "AP African American Studies", "AP African American Studies", SubjectGroup.Humanities, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.Politics, "Politik", "Politics", SubjectGroup.SocialStudies, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.Citizenship, "Politische Bildung", "Citizenship", SubjectGroup.SocialStudies, RoomKind.Library, 1, 7, 0),
            new SubjectSpec(Subject.USGovernment, "US-Regierungssystem", "USGovernment", SubjectGroup.SocialStudies, RoomKind.Library, 2, 4, 0),
            new SubjectSpec(Subject.ComparativeGovernment, "Vergleich politischer Systeme", "Comparative Government", SubjectGroup.SocialStudies, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.SocialStudies, "Gesellschaftslehre", "Social Studies", SubjectGroup.SocialStudies, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.Economics, "Wirtschaft", "Economics", SubjectGroup.SocialStudies, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.BusinessStudies, "Betriebswirtschaft", "Business Studies", SubjectGroup.SocialStudies, RoomKind.ComputerLab, 2, 7, 0),
            new SubjectSpec(Subject.FinancialLiteracy, "Finanzbildung", "Financial Literacy", SubjectGroup.SocialStudies, RoomKind.ComputerLab, 2, 7, 0),
            new SubjectSpec(Subject.Law, "Recht", "Law", SubjectGroup.SocialStudies, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.Psychology, "Psychologie", "Psychology", SubjectGroup.SocialStudies, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.Sociology, "Soziologie", "Sociology", SubjectGroup.SocialStudies, RoomKind.Library, 2, 7, 0),
            new SubjectSpec(Subject.Careers, "Berufsorientierung", "Careers", SubjectGroup.SocialStudies, RoomKind.Classroom, 1, 7, 0),
            new SubjectSpec(Subject.ChildDevelopment, "Entwicklung von Kindern", "Child Development", SubjectGroup.SocialStudies, RoomKind.Classroom, 2, 7, 0),
            new SubjectSpec(Subject.APUSGovernment, "AP USGovernment", "AP USGovernment", SubjectGroup.SocialStudies, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APComparativeGovernment, "AP Comparative Government", "AP Comparative Government", SubjectGroup.SocialStudies, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APMicroeconomics, "AP Microeconomics", "AP Microeconomics", SubjectGroup.SocialStudies, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APMacroeconomics, "AP Macroeconomics", "AP Macroeconomics", SubjectGroup.SocialStudies, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APPsychology, "AP Psychology", "AP Psychology", SubjectGroup.SocialStudies, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APSeminar, "AP Seminar", "AP Seminar", SubjectGroup.SocialStudies, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APResearch, "AP Research", "AP Research", SubjectGroup.SocialStudies, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.Music, "Musik", "Music", SubjectGroup.CreativeArts, RoomKind.MusicRoom, 1, 7, 0),
            new SubjectSpec(Subject.Choir, "Chor", "Choir", SubjectGroup.CreativeArts, RoomKind.MusicRoom, 1, 7, 0),
            new SubjectSpec(Subject.InstrumentalMusic, "Instrumentalmusik", "Instrumental Music", SubjectGroup.CreativeArts, RoomKind.MusicRoom, 2, 7, 0),
            new SubjectSpec(Subject.MusicTheory, "Musiktheorie", "Music Theory", SubjectGroup.CreativeArts, RoomKind.MusicRoom, 2, 7, 0),
            new SubjectSpec(Subject.Drama, "Darstellendes Spiel", "Drama", SubjectGroup.CreativeArts, RoomKind.Theatre, 1, 7, 0),
            new SubjectSpec(Subject.Dance, "Tanz", "Dance", SubjectGroup.CreativeArts, RoomKind.Theatre, 1, 7, 0),
            new SubjectSpec(Subject.FilmStudies, "Filmwissenschaft", "Film Studies", SubjectGroup.CreativeArts, RoomKind.Theatre, 2, 7, 0),
            new SubjectSpec(Subject.Photography, "Fotografie", "Photography", SubjectGroup.CreativeArts, RoomKind.ArtRoom, 2, 7, 0),
            new SubjectSpec(Subject.ArtHistory, "Kunstgeschichte", "Art History", SubjectGroup.CreativeArts, RoomKind.ArtRoom, 2, 7, 0),
            new SubjectSpec(Subject.GraphicDesign, "Grafikdesign", "Graphic Design", SubjectGroup.CreativeArts, RoomKind.ComputerLab, 2, 7, 0),
            new SubjectSpec(Subject.APArtHistory, "AP Art History", "AP Art History", SubjectGroup.CreativeArts, RoomKind.ArtRoom, 3, 4, 2),
            new SubjectSpec(Subject.APMusicTheory, "AP Music Theory", "AP Music Theory", SubjectGroup.CreativeArts, RoomKind.MusicRoom, 3, 4, 2),
            new SubjectSpec(Subject.APArt2D, "AP 2-D Art and Design", "AP 2-D Art and Design", SubjectGroup.CreativeArts, RoomKind.ArtRoom, 3, 4, 2),
            new SubjectSpec(Subject.APArt3D, "AP 3-D Art and Design", "AP 3-D Art and Design", SubjectGroup.CreativeArts, RoomKind.ArtRoom, 3, 4, 2),
            new SubjectSpec(Subject.APDrawing, "AP Drawing", "AP Drawing", SubjectGroup.CreativeArts, RoomKind.ArtRoom, 3, 4, 2),
            new SubjectSpec(Subject.ComputerScience, "Informatik", "Computer Science", SubjectGroup.Computing, RoomKind.ComputerLab, 1, 7, 1),
            new SubjectSpec(Subject.Programming, "Programmierung", "Programming", SubjectGroup.Computing, RoomKind.ComputerLab, 2, 7, 1),
            new SubjectSpec(Subject.DigitalLiteracy, "Digitale Bildung", "Digital Literacy", SubjectGroup.Computing, RoomKind.ComputerLab, 1, 7, 1),
            new SubjectSpec(Subject.Cybersecurity, "IT-Sicherheit", "Cybersecurity", SubjectGroup.Computing, RoomKind.ComputerLab, 3, 7, 2),
            new SubjectSpec(Subject.DataScience, "Datenwissenschaft", "Data Science", SubjectGroup.Computing, RoomKind.ComputerLab, 3, 7, 2),
            new SubjectSpec(Subject.APComputerScienceA, "AP Computer Science A", "AP Computer Science A", SubjectGroup.Computing, RoomKind.ComputerLab, 3, 4, 2),
            new SubjectSpec(Subject.APComputerSciencePrinciples, "AP Computer Science Principles", "AP Computer Science Principles", SubjectGroup.Computing, RoomKind.ComputerLab, 3, 4, 2),
            new SubjectSpec(Subject.MediaStudies, "Medienkunde", "Media Studies", SubjectGroup.Computing, RoomKind.ComputerLab, 2, 7, 1),
            new SubjectSpec(Subject.DesignTechnology, "Design und Technik", "Design Technology", SubjectGroup.Technology, RoomKind.Workshop, 1, 7, 1),
            new SubjectSpec(Subject.Engineering, "Ingenieurtechnik", "Engineering", SubjectGroup.Technology, RoomKind.Workshop, 2, 7, 1),
            new SubjectSpec(Subject.Robotics, "Robotik", "Robotics", SubjectGroup.Technology, RoomKind.Workshop, 2, 7, 1),
            new SubjectSpec(Subject.Woodwork, "Holzarbeiten", "Woodwork", SubjectGroup.Technology, RoomKind.Workshop, 1, 7, 1),
            new SubjectSpec(Subject.Textiles, "Textiles Gestalten", "Textiles", SubjectGroup.Technology, RoomKind.Workshop, 1, 7, 1),
            new SubjectSpec(Subject.Electronics, "Elektronik", "Electronics", SubjectGroup.Technology, RoomKind.Workshop, 2, 7, 1),
            new SubjectSpec(Subject.Construction, "Bautechnik", "Construction", SubjectGroup.Technology, RoomKind.Workshop, 2, 7, 1),
            new SubjectSpec(Subject.Automotive, "Fahrzeugtechnik", "Automotive", SubjectGroup.Technology, RoomKind.Workshop, 2, 7, 1),
            new SubjectSpec(Subject.Agriculture, "Landwirtschaft", "Agriculture", SubjectGroup.Technology, RoomKind.Garden, 2, 7, 1),
            new SubjectSpec(Subject.FoodNutrition, "Ernährung", "Food Nutrition", SubjectGroup.Health, RoomKind.FoodLab, 1, 7, 0),
            new SubjectSpec(Subject.HomeEconomics, "Hauswirtschaft", "Home Economics", SubjectGroup.Health, RoomKind.FoodLab, 1, 7, 0),
            new SubjectSpec(Subject.HealthEducation, "Gesundheit", "Health Education", SubjectGroup.Health, RoomKind.Classroom, 1, 7, 0),
            new SubjectSpec(Subject.RelationshipsHealth, "Beziehungen und Gesundheit", "Relationships Health", SubjectGroup.Health, RoomKind.Classroom, 1, 7, 0),
            new SubjectSpec(Subject.PersonalSocialEducation, "Persönliche und soziale Bildung", "Personal Social Education", SubjectGroup.Health, RoomKind.Classroom, 1, 7, 0),
            new SubjectSpec(Subject.LifeSkills, "Alltagskompetenzen", "Life Skills", SubjectGroup.Health, RoomKind.FoodLab, 1, 7, 0),
            new SubjectSpec(Subject.HealthSocialCare, "Gesundheit und Pflege", "Health Social Care", SubjectGroup.Health, RoomKind.ScienceLab, 2, 7, 0),
            new SubjectSpec(Subject.SportsScience, "Sportwissenschaft", "Sports Science", SubjectGroup.Sports, RoomKind.Gym, 2, 7, 0),
            new SubjectSpec(Subject.OutdoorEducation, "Erlebnispädagogik", "Outdoor Education", SubjectGroup.Sports, RoomKind.Garden, 1, 7, 0),
            new SubjectSpec(Subject.APEnglishLanguage, "AP English Language and Composition", "AP English Language and Composition", SubjectGroup.Languages, RoomKind.LanguageLab, 3, 4, 2),
            new SubjectSpec(Subject.APEnglishLiterature, "AP English Literature and Composition", "AP English Literature and Composition", SubjectGroup.Languages, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APFrench, "AP French Language and Culture", "AP French Language and Culture", SubjectGroup.Languages, RoomKind.LanguageLab, 3, 4, 2),
            new SubjectSpec(Subject.APSpanishLanguage, "AP Spanish Language and Culture", "AP Spanish Language and Culture", SubjectGroup.Languages, RoomKind.LanguageLab, 3, 4, 2),
            new SubjectSpec(Subject.APSpanishLiterature, "AP Spanish Literature and Culture", "AP Spanish Literature and Culture", SubjectGroup.Languages, RoomKind.Library, 3, 4, 2),
            new SubjectSpec(Subject.APGerman, "AP German Language and Culture", "AP German Language and Culture", SubjectGroup.Languages, RoomKind.LanguageLab, 3, 4, 2),
            new SubjectSpec(Subject.APItalian, "AP Italian Language and Culture", "AP Italian Language and Culture", SubjectGroup.Languages, RoomKind.LanguageLab, 3, 4, 2),
            new SubjectSpec(Subject.APJapanese, "AP Japanese Language and Culture", "AP Japanese Language and Culture", SubjectGroup.Languages, RoomKind.LanguageLab, 3, 4, 2),
            new SubjectSpec(Subject.APChinese, "AP Chinese Language and Culture", "AP Chinese Language and Culture", SubjectGroup.Languages, RoomKind.LanguageLab, 3, 4, 2),
            new SubjectSpec(Subject.APLatin, "AP Latin", "AP Latin", SubjectGroup.Languages, RoomKind.LanguageLab, 3, 4, 2),
            new SubjectSpec(Subject.APPrecalculus, "AP Precalculus", "AP Precalculus", SubjectGroup.Mathematics, RoomKind.ComputerLab, 3, 4, 2),
            new SubjectSpec(Subject.APBusinessFinance, "AP Business with Personal Finance", "AP Business with Personal Finance", SubjectGroup.SocialStudies, RoomKind.ComputerLab, 3, 4, 2),
            new SubjectSpec(Subject.APCybersecurity, "AP Cybersecurity", "AP Cybersecurity", SubjectGroup.Computing, RoomKind.ComputerLab, 3, 4, 2),
            new SubjectSpec(Subject.APNetworking, "AP Networking", "AP Networking", SubjectGroup.Computing, RoomKind.ComputerLab, 3, 4, 2)
        };
        public static SubjectSpec Get(Subject id, SchoolState s = null)
        {
            int i=(int)id; if (i>=0 && i<Subjects.Length) return Subjects[i];
            return s == null || s.CustomSubjects == null ? null : s.CustomSubjects.FirstOrDefault(c=>c.Id==id);
        }
        public static bool Valid(Subject id, SchoolState s) { return Get(id,s)!=null; }
        public static IEnumerable<SubjectSpec> All(SchoolState s) { return Subjects.Concat(s.CustomSubjects); }
        public static Subject Foundation(SubjectGroup g)
        { return g==SubjectGroup.Mathematics || g==SubjectGroup.Computing || g==SubjectGroup.Technology ? Subject.Mathematics : g==SubjectGroup.Science ? Subject.Science : g==SubjectGroup.CreativeArts ? Subject.Arts : g==SubjectGroup.Health || g==SubjectGroup.Sports ? Subject.Sports : Subject.Languages; }
        public static List<Subject> RepeatDay(IEnumerable<Subject> lessons) { return Enumerable.Range(0,5).SelectMany(_=>lessons).ToList(); }
        public static List<Subject> Week(CurriculumRegion region)
        {
            switch(region)
            {
                case CurriculumRegion.Germany: return new List<Subject> {
                    Subject.Mathematics,Subject.German,Subject.English,Subject.Biology,Subject.History,Subject.Sports,
                    Subject.German,Subject.Mathematics,Subject.Geography,Subject.Music,Subject.ComputerScience,Subject.Sports,
                    Subject.Mathematics,Subject.English,Subject.Chemistry,Subject.German,Subject.Arts,Subject.Politics,
                    Subject.German,Subject.Mathematics,Subject.Physics,Subject.Ethics,Subject.History,Subject.Sports,
                    Subject.Mathematics,Subject.English,Subject.Biology,Subject.DesignTechnology,Subject.Careers,Subject.Arts };
                case CurriculumRegion.England: return new List<Subject> {
                    Subject.English,Subject.Mathematics,Subject.Science,Subject.History,Subject.French,Subject.Sports,
                    Subject.Mathematics,Subject.English,Subject.Science,Subject.Geography,Subject.ComputerScience,Subject.Music,
                    Subject.English,Subject.Mathematics,Subject.Science,Subject.Arts,Subject.Citizenship,Subject.French,
                    Subject.Mathematics,Subject.English,Subject.Science,Subject.History,Subject.DesignTechnology,Subject.Sports,
                    Subject.Mathematics,Subject.Science,Subject.ReligiousEducation,Subject.ComputerScience,Subject.RelationshipsHealth,Subject.PersonalSocialEducation };
                case CurriculumRegion.USA: return new List<Subject> {
                    Subject.English,Subject.Mathematics,Subject.Biology,Subject.USHistory,Subject.Spanish,Subject.Sports,
                    Subject.English,Subject.Mathematics,Subject.Science,Subject.USGovernment,Subject.ComputerScience,Subject.Arts,
                    Subject.English,Subject.Mathematics,Subject.Chemistry,Subject.WorldHistory,Subject.Spanish,Subject.HealthEducation,
                    Subject.English,Subject.Mathematics,Subject.Physics,Subject.SocialStudies,Subject.ComputerScience,Subject.Sports,
                    Subject.English,Subject.Mathematics,Subject.EnvironmentalScience,Subject.FinancialLiteracy,Subject.Spanish,Subject.Arts };
                default: return RepeatDay(new[]{Subject.Mathematics,Subject.Languages,Subject.Science,Subject.Arts,Subject.Sports,Subject.Mathematics});
            }
        }
    }
}
