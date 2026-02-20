import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl + '/Auth';
  private currentUserSubject: BehaviorSubject<string | null>;
  public currentUser: Observable<string | null>;

  constructor(private http: HttpClient) {
    const storedUser = localStorage.getItem('username');
    this.currentUserSubject = new BehaviorSubject<string | null>(storedUser);
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): string | null {
    return this.currentUserSubject.value;
  }

  login(username: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, { username, password }, { withCredentials: true })
      .pipe(
        tap(response => {
          if (response && response.username) {
            localStorage.setItem('username', response.username);
            this.currentUserSubject.next(response.username);
          }
        }),
        catchError(this.handleError)
      );
  }

  logout(): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/logout`, {}, { withCredentials: true })
      .pipe(
        tap(() => {
          localStorage.removeItem('username');
          this.currentUserSubject.next(null);
        }),
        catchError(this.handleError)
      );
  }
  
  // Clean localSession if cookie is gone (called from interceptor maybe, but simple for now)
  forceLogout() {
      localStorage.removeItem('username');
      this.currentUserSubject.next(null);
  }

  private handleError(error: any) {
    console.error('Error occurred', error);
    return throwError(() => error);
  }
}
