import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, catchError, Observable, of, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
}

export interface AuthResponse {
  token: string;
  refreshToken?: string;
  expiresAt?: Date | string;
  user: User;
}

export interface User {
  id: string;
  name: string;
  email: string;
  isActive: boolean;
  createdAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'auth_user';
  private readonly EXPIRY_KEY = 'auth_expiry';

  private currentUserSubject = new BehaviorSubject<User | null>(this.getSavedUser());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) { }

  register(credentials: RegisterRequest): Observable<{ userId: string; email: string }> {
    return this.http.post<{ userId: string; email: string }>(`${environment.apiUrl}/api/v1/auth/register`, credentials);
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/api/v1/auth/login`, credentials)
      .pipe(
        tap(response => {
          this.saveAuthData(response);
          this.currentUserSubject.next(response.user);
        })
      );
  }

  logout(): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/v1/auth/logout`, {}).pipe(
      tap(() => {
        this.clearAuthData();
        this.currentUserSubject.next(null);
        this.router.navigate(['/login']);
      }),
      catchError((error) => {
        this.clearAuthData();
        this.currentUserSubject.next(null);
        this.router.navigate(['/login']);
        return of(null);
      })
    );
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  getCurrentUserId(): string | null {
    const user = this.getCurrentUser();
    return user ? user.id : null;
  }

  private saveAuthData(response: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.token);
    
    if (response.refreshToken) {
      localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    }
    
    if (response.user) {
      localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));
    }
    
    if (response.expiresAt) {
      const expiryDate = typeof response.expiresAt === 'string' 
        ? response.expiresAt 
        : response.expiresAt.toISOString();
      localStorage.setItem(this.EXPIRY_KEY, expiryDate);
    } else {
      const defaultExpiry = new Date();
      defaultExpiry.setHours(defaultExpiry.getHours() + 1);
      localStorage.setItem(this.EXPIRY_KEY, defaultExpiry.toISOString());
    }
  }

  private clearAuthData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.EXPIRY_KEY);
  }

  private getSavedUser(): User | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    if (userJson) {
      try {
        return JSON.parse(userJson);
      } catch {
        this.clearAuthData();
        return null;
      }
    }
    return null;
  }

  private getExpiry(): string | null {
    return localStorage.getItem(this.EXPIRY_KEY);
  }
}
