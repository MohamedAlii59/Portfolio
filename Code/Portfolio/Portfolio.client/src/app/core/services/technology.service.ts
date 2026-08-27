import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Technology } from '../models/technology.models';

@Injectable({ providedIn: 'root' })
export class TechnologyService {
  private readonly apiUrl = `${environment.apiUrl}/technologies`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Technology[]> {
    return this.http.get<Technology[]>(this.apiUrl);
  }

  create(name: string, icon: File | null): Observable<Technology> {
    const formData = new FormData();
    formData.append('Name', name);
    if (icon) formData.append('Icon', icon);
    return this.http.post<Technology>(this.apiUrl, formData);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getProfileTechnologies(userId: number): Observable<Technology[]> {
    return this.http.get<Technology[]>(`${this.apiUrl}/profile/${userId}`);
  }

  addToProfile(technologyId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/profile/${technologyId}`, {});
  }

  removeFromProfile(technologyId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/profile/${technologyId}`);
  }
}