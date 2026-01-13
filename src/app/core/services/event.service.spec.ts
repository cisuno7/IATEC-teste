import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EventService, Event, EventFilters, CreateEventRequest } from './event.service';
import { environment } from '../../../environments/environment';

describe('EventService', () => {
  let service: EventService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [EventService]
    });
    service = TestBed.inject(EventService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getDashboardEvents', () => {
    it('should fetch events without filters', () => {
      const mockEvents: Event[] = [
        {
          id: '1',
          name: 'Test Event',
          description: 'Test Description',
          date: new Date('2024-01-01'),
          location: 'Test Location',
          type: 0,
          isActive: true,
          createdAt: new Date('2024-01-01'),
          creatorId: '1',
          creatorName: 'Test User',
          participants: []
        }
      ];

      service.getDashboardEvents().subscribe(events => {
        expect(events).toEqual(mockEvents);
        expect(events.length).toBe(1);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/events/dashboard`);
      expect(req.request.method).toBe('GET');
      req.flush(mockEvents);
    });

    it('should fetch events with filters', () => {
      const filters: EventFilters = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-31'),
        searchText: 'test',
        periodType: 'month'
      };

      const mockEvents: Event[] = [];

      service.getDashboardEvents(filters).subscribe(events => {
        expect(events).toEqual(mockEvents);
      });

      const req = httpMock.expectOne(
        req => req.url === `${environment.apiUrl}/api/v1/events/dashboard` &&
               req.params.get('startDate') !== null &&
               req.params.get('endDate') !== null &&
               req.params.get('searchText') === 'test' &&
               req.params.get('periodType') === 'month'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockEvents);
    });
  });

  describe('createEvent', () => {
    it('should create an event', () => {
      const newEvent: CreateEventRequest = {
        name: 'New Event',
        description: 'New Description',
        date: new Date('2024-02-01'),
        location: 'New Location',
        type: 0,
        participantIds: []
      };

      const mockEvent: Event = {
        id: '2',
        name: 'New Event',
        description: 'New Description',
        date: new Date('2024-02-01'),
        location: 'New Location',
        type: 0,
        isActive: true,
        createdAt: new Date('2024-01-01'),
        creatorId: '1',
        creatorName: 'Test User',
        participants: []
      };

      service.createEvent(newEvent).subscribe(event => {
        expect(event).toEqual(mockEvent);
        expect(event.name).toBe('New Event');
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/events`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newEvent);
      req.flush(mockEvent);
    });
  });

  describe('updateEvent', () => {
    it('should update an event', () => {
      const updateRequest = {
        id: '1',
        name: 'Updated Event',
        description: 'Updated Description',
        date: new Date('2024-02-01'),
        location: 'Updated Location',
        type: 0,
        participantIds: []
      };

      const mockEvent: Event = {
        id: '1',
        name: 'Updated Event',
        description: 'Updated Description',
        date: new Date('2024-02-01'),
        location: 'Updated Location',
        type: 0,
        isActive: true,
        createdAt: new Date('2024-01-01'),
        creatorId: '1',
        creatorName: 'Test User',
        participants: []
      };

      service.updateEvent(updateRequest).subscribe(event => {
        expect(event).toEqual(mockEvent);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/events/${updateRequest.id}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateRequest);
      req.flush(mockEvent);
    });
  });

  describe('deleteEvent', () => {
    it('should delete an event', () => {
      const eventId = '1';

      service.deleteEvent(eventId).subscribe(() => {
        expect(true).toBeTruthy();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/events/${eventId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('activateEvent', () => {
    it('should activate an event', () => {
      const eventId = '1';

      service.activateEvent(eventId).subscribe(() => {
        expect(true).toBeTruthy();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/events/${eventId}/activate`);
      expect(req.request.method).toBe('PATCH');
      req.flush(null);
    });
  });

  describe('deactivateEvent', () => {
    it('should deactivate an event', () => {
      const eventId = '1';

      service.deactivateEvent(eventId).subscribe(() => {
        expect(true).toBeTruthy();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/events/${eventId}/deactivate`);
      expect(req.request.method).toBe('PATCH');
      req.flush(null);
    });
  });

  describe('addParticipant', () => {
    it('should add a participant to an event', () => {
      const eventId = '1';
      const userId = '2';

      service.addParticipant(eventId, userId).subscribe(() => {
        expect(true).toBeTruthy();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/events/${eventId}/participants/${userId}`);
      expect(req.request.method).toBe('POST');
      req.flush(null);
    });
  });

  describe('removeParticipant', () => {
    it('should remove a participant from an event', () => {
      const eventId = '1';
      const userId = '2';

      service.removeParticipant(eventId, userId).subscribe(() => {
        expect(true).toBeTruthy();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/events/${eventId}/participants/${userId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('getActiveUsers', () => {
    it('should fetch active users', () => {
      const mockUsers = [
        {
          id: '1',
          name: 'User 1',
          email: 'user1@test.com',
          isActive: true,
          createdAt: new Date()
        },
        {
          id: '2',
          name: 'User 2',
          email: 'user2@test.com',
          isActive: true,
          createdAt: new Date()
        }
      ];

      service.getActiveUsers().subscribe(users => {
        expect(users).toEqual(mockUsers);
        expect(users.length).toBe(2);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/users/active`);
      expect(req.request.method).toBe('GET');
      req.flush(mockUsers);
    });
  });
});
