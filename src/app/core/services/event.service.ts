import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Event {
  id: string;
  name: string;
  description: string;
  date: Date;
  location: string;
  type: EventType;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
  creatorId: string;
  creatorName: string;
  participants: Participant[];
}

export interface Participant {
  id: string;
  name: string;
  email: string;
}

export enum EventType {
  Exclusive = 0,
  Shared = 1
}

export interface CreateEventRequest {
  name: string;
  description: string;
  date: Date;
  location: string;
  type: EventType;
  participantIds: string[];
}

export interface UpdateEventRequest {
  id: string;
  name: string;
  description: string;
  date: Date;
  location: string;
  type: EventType;
  participantIds: string[];
}

export interface EventFilters {
  startDate?: Date;
  endDate?: Date;
  startTime?: string;
  endTime?: string;
  searchText?: string;
  periodType?: 'today' | 'week' | 'month';
}

@Injectable({
  providedIn: 'root'
})
export class EventService {

  constructor(private http: HttpClient) { }

  getDashboardEvents(filters?: EventFilters): Observable<Event[]> {
    let params = new HttpParams();

    if (filters?.startDate) {
      params = params.set('startDate', filters.startDate.toISOString());
    }

    if (filters?.endDate) {
      params = params.set('endDate', filters.endDate.toISOString());
    }

    if (filters?.startTime) {
      params = params.set('startTime', filters.startTime);
    }

    if (filters?.endTime) {
      params = params.set('endTime', filters.endTime);
    }

    if (filters?.searchText) {
      params = params.set('searchText', filters.searchText);
    }

    if (filters?.periodType) {
      params = params.set('periodType', filters.periodType);
    }

    return this.http.get<Event[]>(`${environment.apiUrl}/api/v1/events/dashboard`, { params });
  }

  createEvent(event: CreateEventRequest): Observable<Event> {
    return this.http.post<Event>(`${environment.apiUrl}/api/v1/events`, event);
  }

  updateEvent(event: UpdateEventRequest): Observable<Event> {
    return this.http.put<Event>(`${environment.apiUrl}/api/v1/events/${event.id}`, event);
  }

  deleteEvent(eventId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/api/v1/events/${eventId}`);
  }

  activateEvent(eventId: string): Observable<void> {
    return this.http.patch<void>(`${environment.apiUrl}/api/v1/events/${eventId}/activate`, {});
  }

  deactivateEvent(eventId: string): Observable<void> {
    return this.http.patch<void>(`${environment.apiUrl}/api/v1/events/${eventId}/deactivate`, {});
  }

  addParticipant(eventId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/api/v1/events/${eventId}/participants/${userId}`, {});
  }

  removeParticipant(eventId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/api/v1/events/${eventId}/participants/${userId}`);
  }

  getActiveUsers(): Observable<UserOption[]> {
    return this.http.get<UserOption[]>(`${environment.apiUrl}/api/v1/users/active`);
  }
}

export interface UserOption {
  id: string;
  name: string;
  email: string;
  isActive: boolean;
  createdAt: Date;
}
