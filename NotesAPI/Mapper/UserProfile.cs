using AutoMapper;
using NotesAPI.Dto;
using NotesAPI.Models;

namespace NotesAPI.Mapper
{
    public class UserProfile: Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterRequest, User>();
        }
    }
}
