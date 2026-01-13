import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { EventModalComponent } from './event-modal.component';
import { EventService, Event, EventType, UserOption } from '../../../../core/services/event.service';
import { AuthService } from '../../../../core/auth/auth.service';

describe('EventModalComponent', () => {
  let component: EventModalComponent;
  let fixture: ComponentFixture<EventModalComponent>;
  let eventService: jasmine.SpyObj<EventService>;
  let authService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    const eventServiceSpy = jasmine.createSpyObj('EventService', ['createEvent', 'updateEvent', 'getActiveUsers']);
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['getCurrentUserId']);

    await TestBed.configureTestingModule({
      declarations: [EventModalComponent],
      imports: [ReactiveFormsModule, FormsModule],
      providers: [
        { provide: EventService, useValue: eventServiceSpy },
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    eventService = TestBed.inject(EventService) as jasmine.SpyObj<EventService>;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EventModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should disable save button when form is invalid', () => {
    component.eventForm.patchValue({
      name: '',
      description: '',
      date: '',
      time: '',
      location: '',
      type: EventType.Exclusive
    });

    expect(component.eventForm.invalid).toBeTrue();
  });

  it('should enable save button when form is valid', () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    const dateStr = futureDate.toISOString().split('T')[0];

    component.eventForm.patchValue({
      name: 'Test Event',
      description: 'Test Description',
      date: dateStr,
      time: '10:00',
      location: 'Test Location',
      type: EventType.Exclusive
    });

    expect(component.eventForm.valid).toBeTrue();
  });

  it('should call createEvent when creating new event', () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    const dateStr = futureDate.toISOString().split('T')[0];

    component.isEditMode = false;
    component.eventForm.patchValue({
      name: 'New Event',
      description: 'Description',
      date: dateStr,
      time: '10:00',
      location: 'Location',
      type: EventType.Exclusive
    });

    const mockEvent: Event = {
      id: '1',
      name: 'New Event',
      description: 'Description',
      date: futureDate,
      location: 'Location',
      type: EventType.Exclusive,
      isActive: true,
      createdAt: new Date(),
      creatorId: '1',
      creatorName: 'Creator',
      participants: []
    };

    eventService.createEvent.and.returnValue(of(mockEvent));
    spyOn(component.eventSaved, 'emit');

    component.onSubmit();

    expect(eventService.createEvent).toHaveBeenCalled();
    expect(component.eventSaved.emit).toHaveBeenCalledWith(mockEvent);
  });

  it('should call updateEvent when editing existing event', () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    const dateStr = futureDate.toISOString().split('T')[0];

    component.isEditMode = true;
    component.event = {
      id: '1',
      name: 'Event',
      description: 'Description',
      date: futureDate,
      location: 'Location',
      type: EventType.Exclusive,
      isActive: true,
      createdAt: new Date(),
      creatorId: '1',
      creatorName: 'Creator',
      participants: []
    };

    component.eventForm.patchValue({
      name: 'Updated Event',
      description: 'Updated Description',
      date: dateStr,
      time: '11:00',
      location: 'Updated Location',
      type: EventType.Exclusive
    });

    const mockEvent: Event = {
      id: '1',
      name: 'Updated Event',
      description: 'Updated Description',
      date: futureDate,
      location: 'Updated Location',
      type: EventType.Exclusive,
      isActive: true,
      createdAt: new Date(),
      creatorId: '1',
      creatorName: 'Creator',
      participants: []
    };

    eventService.updateEvent.and.returnValue(of(mockEvent));
    spyOn(component.eventSaved, 'emit');

    component.onSubmit();

    expect(eventService.updateEvent).toHaveBeenCalled();
    expect(component.eventSaved.emit).toHaveBeenCalledWith(mockEvent);
  });

  it('should not submit when form is invalid', () => {
    component.eventForm.patchValue({
      name: '',
      description: '',
      date: '',
      time: '',
      location: '',
      type: EventType.Exclusive
    });

    component.onSubmit();

    expect(eventService.createEvent).not.toHaveBeenCalled();
    expect(eventService.updateEvent).not.toHaveBeenCalled();
  });

  it('should display error message on create failure', () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    const dateStr = futureDate.toISOString().split('T')[0];

    component.isEditMode = false;
    component.eventForm.patchValue({
      name: 'Event',
      description: 'Description',
      date: dateStr,
      time: '10:00',
      location: 'Location',
      type: EventType.Exclusive
    });

    eventService.createEvent.and.returnValue(
      throwError(() => ({ error: { message: 'Erro ao criar evento' } }))
    );

    component.onSubmit();

    expect(component.errorMessage).toBe('Erro ao criar evento');
    expect(component.isLoading).toBeFalse();
  });

  it('should load available users on modal open', () => {
    const mockUsers: UserOption[] = [
      {
        id: '1',
        name: 'User 1',
        email: 'user1@test.com',
        isActive: true,
        createdAt: new Date()
      }
    ];

    eventService.getActiveUsers.and.returnValue(of(mockUsers));

    component.isOpen = true;
    component.ngOnChanges({
      isOpen: {
        currentValue: true,
        previousValue: false,
        firstChange: false,
        isFirstChange: () => false
      }
    });

    expect(eventService.getActiveUsers).toHaveBeenCalled();
  });
});
