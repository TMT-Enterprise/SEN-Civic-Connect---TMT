import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { RequestCard } from '../../../shared/request-card/request-card';

@Component({
  selector: 'app-requester-home',
  standalone: true,
  imports: [
    RouterLink,
    RequestCard
  ],
  templateUrl: './requester-home.html',
  styleUrl: './requester-home.css',
})
export class RequesterHome {}
