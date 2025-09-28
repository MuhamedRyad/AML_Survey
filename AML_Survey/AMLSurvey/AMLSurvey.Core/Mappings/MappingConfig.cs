
using Mapster;

namespace AMLSurvey.Core.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {   
        //NewConfig<Src, dest>()
       /* config.NewConfig<Student, StudentResponse>()
            .Map(dest => dest.FullName, src => $"{src.FirstName} {src.MiddleName ?? ""} {src.LastName}".Trim())

            .Map(dest => dest.Age, src =>
                src.DateOfBirth.HasValue ? DateTime.Now.Year - src.DateOfBirth.Value.Year : 0)
            .Ignore(dest => dest.DepartmentName)
            .TwoWays(); // ????? ??????? ?? ?????????

        config.NewConfig<StudentResponse, Student>()
            .Map(dest => dest.FirstName, src => src.FullName.Split(' ')[0])
            .Map(dest => dest.LastName, src =>
                src.FullName.Split(' ').Length > 1 ? src.FullName.Split(' ')[^1] : "")
            .Map(dest => dest.MiddleName, src =>
                src.FullName.Split(' ').Length > 2 ? string.Join(" ", src.FullName.Split(' ').Skip(1).Take(src.FullName.Split(' ').Length - 2)) : null);*/
    }
}
