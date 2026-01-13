import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService, LoginRequest, RegisterRequest, AuthResponse } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        { provide: Router, useValue: routerSpy }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should login and store token', () => {
      const credentials: LoginRequest = {
        email: 'test@example.com',
        password: 'password123'
      };

      const mockResponse: AuthResponse = {
        token: 'mock-token',
        user: {
          id: '1',
          name: 'Test User',
          email: 'test@example.com',
          isActive: true,
          createdAt: new Date()
        }
      };

      service.login(credentials).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(service.getToken()).toBe('mock-token');
        expect(service.getCurrentUser()).toEqual(mockResponse.user);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/auth/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(credentials);
      req.flush(mockResponse);
    });

    it('should update currentUser$ observable on login', (done) => {
      const credentials: LoginRequest = {
        email: 'test@example.com',
        password: 'password123'
      };

      const mockResponse: AuthResponse = {
        token: 'mock-token',
        user: {
          id: '1',
          name: 'Test User',
          email: 'test@example.com',
          isActive: true,
          createdAt: new Date()
        }
      };

      service.currentUser$.subscribe(user => {
        if (user) {
          expect(user.id).toBe('1');
          done();
        }
      });

      service.login(credentials).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/auth/login`);
      req.flush(mockResponse);
    });
  });

  describe('logout', () => {
    it('should clear auth data and navigate to login', () => {
      localStorage.setItem('auth_token', 'token');
      localStorage.setItem('auth_user', JSON.stringify({ id: '1', name: 'User' }));

      service.logout().subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/auth/logout`);
      expect(req.request.method).toBe('POST');
      req.flush({});

      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should clear auth data even if logout request fails', () => {
      localStorage.setItem('auth_token', 'token');

      service.logout().subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/auth/logout`);
      req.error(new ErrorEvent('Network error'));

      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('register', () => {
    it('should register new user', () => {
      const registerData: RegisterRequest = {
        email: 'new@example.com',
        password: 'password123',
        name: 'New User'
      };

      const mockResponse = {
        userId: '2',
        email: 'new@example.com'
      };

      service.register(registerData).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/v1/auth/register`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(registerData);
      req.flush(mockResponse);
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when token exists', () => {
      localStorage.setItem('auth_token', 'token');

      expect(service.isAuthenticated()).toBeTrue();
    });

    it('should return false when token does not exist', () => {
      expect(service.isAuthenticated()).toBeFalse();
    });
  });

  describe('getToken', () => {
    it('should return token from localStorage', () => {
      localStorage.setItem('auth_token', 'test-token');

      expect(service.getToken()).toBe('test-token');
    });

    it('should return null when token does not exist', () => {
      expect(service.getToken()).toBeNull();
    });
  });

  describe('getCurrentUser', () => {
    it('should return current user from observable', () => {
      const mockDate = new Date('2026-01-12T19:34:03.468Z');
      const user = {
        id: '1',
        name: 'Test User',
        email: 'test@example.com',
        isActive: true,
        createdAt: mockDate
      };

      // Set localStorage before creating a new service instance
      localStorage.setItem('auth_user', JSON.stringify(user));
      
      // Create a new service instance to pick up the localStorage value
      const httpClient = TestBed.inject(HttpClient);
      const router = TestBed.inject(Router);
      const testService = new AuthService(httpClient, router);

      const currentUser = testService.getCurrentUser();
      
      // Compare dates using ISO string to avoid timezone issues
      expect(currentUser).toBeTruthy();
      expect(currentUser?.id).toBe(user.id);
      expect(currentUser?.name).toBe(user.name);
      expect(currentUser?.email).toBe(user.email);
      expect(currentUser?.isActive).toBe(user.isActive);
      if (currentUser?.createdAt) {
        expect(new Date(currentUser.createdAt).toISOString()).toBe(mockDate.toISOString());
      }
    });

    it('should return null when no user is stored', () => {
      expect(service.getCurrentUser()).toBeNull();
    });
  });

  describe('getCurrentUserId', () => {
    it('should return user id when user exists', () => {
      const user = {
        id: '1',
        name: 'Test User',
        email: 'test@example.com',
        isActive: true,
        createdAt: new Date()
      };

      // Set localStorage before creating a new service instance
      localStorage.setItem('auth_user', JSON.stringify(user));
      
      // Create a new service instance to pick up the localStorage value
      const httpClient = TestBed.inject(HttpClient);
      const router = TestBed.inject(Router);
      const testService = new AuthService(httpClient, router);

      expect(testService.getCurrentUserId()).toBe('1');
    });

    it('should return null when no user exists', () => {
      expect(service.getCurrentUserId()).toBeNull();
    });
  });
});
