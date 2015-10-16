namespace Companynamehere.Project.UI.Services.Mocks
{
    using System;

    using Companynamehere.Outsourcing.Shared.SharedLibrary;

    public interface IMockDataService
    {
        void WriteObject<TObject>(TObject instance, string filename = null, ICallerContext callerContext = null);

        TObject ReadObject<TObject>(Func<TObject> job, string filename = null, ICallerContext callerContext = null);

        TObject DeserializeObject<TObject>(string jsonString);

        TObject ReadObject<TObject>(string filename = null, ICallerContext callerContext = null);
    }
}