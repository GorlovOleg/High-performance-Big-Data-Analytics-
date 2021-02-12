import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule } from "@angular/forms";
import { HttpModule } from '@angular/http';
import { PersonSearchCommonComponent } from './personsearchcommon.component'; 
import { PersonSearchCommonService } from "./Services/personsearchcommon.service";

@NgModule({
    imports: [
        BrowserModule, HttpModule, ReactiveFormsModule
    ],
  bootstrap: [PersonSearchCommonComponent],
  providers: [PersonSearchCommonService]
})

export class PersonSearchCommonModule { }
