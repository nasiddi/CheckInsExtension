﻿namespace KidsTown.BackgroundTasks.Models
{
    public class PeopleUpdate
    {
        public readonly long? PeopleId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly bool MayLeaveAlone;
        public readonly bool HasPeopleWithoutPickupPermission;

        public PeopleUpdate(
            long? peopleId, 
            string firstName, 
            string lastName, 
            bool mayLeaveAlone,
            bool hasPeopleWithoutPickupPermission)
        {
            PeopleId = peopleId;
            FirstName = firstName;
            LastName = lastName;
            MayLeaveAlone = mayLeaveAlone;
            HasPeopleWithoutPickupPermission = hasPeopleWithoutPickupPermission;
        }
    }
}