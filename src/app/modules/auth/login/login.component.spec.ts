import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService, LoginRequest, AuthResponse } from '../../../core/auth/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with empty values', () => {
    expect(component.loginForm.get('email')?.value).toBe('');
    expect(component.loginForm.get('password')?.value).toBe('');
  });

  it('should mark form as invalid when fields are empty', () => {
    expect(component.loginForm.valid).toBeFalsy();
    expect(component.loginForm.get('email')?.hasError('required')).toBeTruthy();
    expect(component.loginForm.get('password')?.hasError('required')).toBeTruthy();
  });

  it('should validate email format', () => {
    const emailControl = component.loginForm.get('email');
    emailControl?.setValue('invalid-email');
    expect(emailControl?.hasError('email')).toBeTruthy();

    emailControl?.setValue('valid@email.com');
    expect(emailControl?.hasError('email')).toBeFalsy();
  });

  it('should validate password minimum length', () => {
    const passwordControl = component.loginForm.get('password');
    passwordControl?.setValue('12345');
    expect(passwordControl?.hasError('minlength')).toBeTruthy();

    passwordControl?.setValue('123456');
    expect(passwordControl?.hasError('minlength')).toBeFalsy();
  });

  it('should call authService.login when form is valid', () => {
    const credentials: LoginRequest = {
      email: 'test@example.com',
      password: 'password123'
    };

    const mockAuthResponse: AuthResponse = {
      token: 'fake-token',
      user: {
        id: '123',
        name: 'Test User',
        email: 'test@example.com',
        isActive: true,
        createdAt: new Date()
      }
    };

    component.loginForm.patchValue(credentials);
    authService.login.and.returnValue(of(mockAuthResponse));

    component.onSubmit();

    expect(authService.login).toHaveBeenCalledWith(credentials);
  });

  it('should navigate to dashboard on successful login', () => {
    const credentials: LoginRequest = {
      email: 'test@example.com',
      password: 'password123'
    };

    const mockAuthResponse: AuthResponse = {
      token: 'fake-token',
      user: {
        id: '123',
        name: 'Test User',
        email: 'test@example.com',
        isActive: true,
        createdAt: new Date()
      }
    };

    component.loginForm.patchValue(credentials);
    authService.login.and.returnValue(of(mockAuthResponse));

    component.onSubmit();

    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should display error message on login failure', () => {
    const credentials: LoginRequest = {
      email: 'test@example.com',
      password: 'wrongpassword'
    };

    component.loginForm.patchValue(credentials);
    authService.login.and.returnValue(
      throwError(() => ({ error: { message: 'Credenciais inválidas' } }))
    );

    component.onSubmit();

    expect(component.errorMessage).toBe('Credenciais inválidas');
    expect(component.isLoading).toBeFalsy();
  });

  it('should not submit form when invalid', () => {
    component.loginForm.patchValue({
      email: '',
      password: ''
    });

    component.onSubmit();

    expect(authService.login).not.toHaveBeenCalled();
  });

  it('should mark all fields as touched when form is invalid and submitted', () => {
    component.loginForm.patchValue({
      email: '',
      password: ''
    });

    component.onSubmit();

    expect(component.loginForm.get('email')?.touched).toBeTruthy();
    expect(component.loginForm.get('password')?.touched).toBeTruthy();
  });

  it('should set isLoading to true when submitting', () => {
    const credentials: LoginRequest = {
      email: 'test@example.com',
      password: 'password123'
    };

    const mockAuthResponse: AuthResponse = {
      token: 'fake-token',
      user: {
        id: '123',
        name: 'Test User',
        email: 'test@example.com',
        isActive: true,
        createdAt: new Date()
      }
    };

    component.loginForm.patchValue(credentials);
    authService.login.and.returnValue(of(mockAuthResponse));

    component.onSubmit();

    expect(component.isLoading).toBeTruthy();
  });
});
