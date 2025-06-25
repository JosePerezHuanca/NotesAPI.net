using AutoMapper;
using NotesAPI.Dto;
using NotesAPI.Models;

namespace NotesAPI.Mapper
{
    public class NoteProfile: Profile
    {
        public NoteProfile()
        {
            CreateMap<Note, NoteResponse>()
                .ForMember(dest => dest.Autor, opt => opt.MapFrom(src => src.User.Username));
            CreateMap<NoteDto, Note>();
        }
    }
}
