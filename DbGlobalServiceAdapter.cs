namespace Companynamehere.Project.UI.Services.Mocks
{
    using System;

    using global::DB.Services.ApplicationEvents;
    using Companynamehere.Project.Services.Shared.BusinessServices.Response;
    using Companynamehere.Outsourcing.Shared.Entities.ParticipantObjectModel.BusinessObjects;
    using Companynamehere.Outsourcing.Shared.Entities.ParticipantObjectModel.Interfaces;
    using Companynamehere.Outsourcing.Shared.EventManagement;
    using Companynamehere.Outsourcing.Shared.SharedLibrary;

    public class DbGlobalServiceAdapter : IEventBusComponent, IDisposable
    {
        private readonly IMockDataService _mockDataService;

        private IEventBus _eventBus;

        public DbGlobalServiceAdapter(IMockDataService mockDataService)
        {
            _mockDataService = mockDataService;
        }

        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe(typeof(GetGlobalWebModelApplicationEvent), GetGlobalWebModelApplicationEventHandler);
            _eventBus.Subscribe(typeof(GetParticipantApplicationEvent), GetParticipantApplicationEventHandler);
        }

        public void Dispose()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe(typeof(GetGlobalWebModelApplicationEvent), GetGlobalWebModelApplicationEventHandler);
                _eventBus.Unsubscribe(typeof(GetParticipantApplicationEvent), GetParticipantApplicationEventHandler);
                _eventBus = null;
            }
        }

        private void GetGlobalWebModelApplicationEventHandler(IApplicationEvent applicationEvent)
        {
            var data = (GetGlobalWebModelApplicationEvent)applicationEvent;
            data.GlobalWebModel = _mockDataService.ReadObject(GetGlobalModel, MockFileName.GlobalModel, data.CallerContext);
        }

        private void GetParticipantApplicationEventHandler(IApplicationEvent applicationEvent)
        {
            var data = (GetParticipantApplicationEvent)applicationEvent;
            var global = _mockDataService.ReadObject(GetGlobalModel, MockFileName.GlobalModel, data.CallerContext);
            if (global != null)
            {
                data.DBParticipant = global.DomainObject;
            }
        }

        private ServiceWebModel<IDBParticipant> GetGlobalModel()
        {
            return _mockDataService.DeserializeObject<ServiceWebModel<IDBParticipant>>(JsonSample.GlobalWebModelJsonString);
        }
    }
}
