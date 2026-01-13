import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { Subject, of, throwError } from 'rxjs';
import { DashboardPageComponent } from './dashboard.page';
import { AuthService, User } from '../../../core/auth/auth.service';
import { EventService, Event, EventFilters } from '../../../core/services/event.service';

describe('DashboardPageComponent', () => {
  let component: DashboardPageComponent;
  let fixture: ComponentFixture<DashboardPageComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let eventService: jasmine.SpyObj<EventService>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['logout', 'currentUser$'], {
      currentUser$: new Subject<User | null>()
    });
    const eventServiceSpy = jasmine.createSpyObj<EventService>('EventService', [
      'getDashboardEvents',
      'deleteEvent',
      'activateEvent',
      'deactivateEvent',
      'removeParticipant'
    ]);

    // Set default return value for getDashboardEvents to prevent pipe errors
    eventServiceSpy.getDashboardEvents.and.returnValue(of([]));
    eventServiceSpy.deleteEvent.and.returnValue(of(undefined as void));
    eventServiceSpy.activateEvent.and.returnValue(of(undefined as void));
    eventServiceSpy.deactivateEvent.and.returnValue(of(undefined as void));
    eventServiceSpy.removeParticipant.and.returnValue(of(undefined as void));

    await TestBed.configureTestingModule({
      declarations: [DashboardPageComponent],
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: EventService, useValue: eventServiceSpy }
      ]
    }).compileComponents();

    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    eventService = TestBed.inject(EventService) as jasmine.SpyObj<EventService>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DashboardPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load events on init', () => {
    const mockEvents: Event[] = [
      {
        id: '1',
        name: 'Test Event',
        description: 'Description',
        date: new Date(),
        location: 'Location',
        type: 0,
        isActive: true,
        createdAt: new Date(),
        creatorId: '1',
        creatorName: 'Creator',
        participants: []
      }
    ];

    eventService.getDashboardEvents.and.returnValue(of(mockEvents));

    component.ngOnInit();

    expect(eventService.getDashboardEvents).toHaveBeenCalled();
  });

  it('should filter events by text', () => {
    const filters: EventFilters = { searchText: 'test' };
    const mockEvents: Event[] = [];

    eventService.getDashboardEvents.and.returnValue(of(mockEvents));

    component.onFiltersChanged(filters);

    expect(component.currentFilters).toEqual(filters);
    expect(eventService.getDashboardEvents).toHaveBeenCalledWith(filters);
  });

  it('should select period and clear manual dates', () => {
    component.currentFilters = {
      startDate: new Date('2024-01-01'),
      endDate: new Date('2024-01-31')
    };

    const filters: EventFilters = { periodType: 'today' };
    const mockEvents: Event[] = [];
    eventService.getDashboardEvents.and.returnValue(of(mockEvents));

    component.onFiltersChanged(filters);

    expect(component.currentFilters.periodType).toBe('today');
    expect(component.currentFilters.startDate).toBeUndefined();
    expect(component.currentFilters.endDate).toBeUndefined();
  });

  it('should display error message for invalid date range', () => {
    const error = {
      status: 400,
      error: { message: 'Intervalo de datas inválido' }
    };

    eventService.getDashboardEvents.and.returnValue(throwError(() => error));

    component.loadEvents();

    expect(component.errorMessage).toBe('Intervalo de datas inválido');
    expect(component.isLoading).toBeFalse();
  });

  it('should check if user can manage event', () => {
    const currentUser: User = {
      id: '1',
      name: 'User',
      email: 'user@test.com',
      isActive: true,
      createdAt: new Date()
    };

    component.currentUser = currentUser;

    const event: Event = {
      id: '1',
      name: 'Event',
      description: 'Description',
      date: new Date(),
      location: 'Location',
      type: 0,
      isActive: true,
      createdAt: new Date(),
      creatorId: '1',
      creatorName: 'Creator',
      participants: []
    };

    expect(component.canManageEvent(event)).toBeTrue();

    event.creatorId = '2';
    expect(component.canManageEvent(event)).toBeFalse();
  });
});
