import { Component, NgModule, OnInit} from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { DxBoxModule, DxButtonModule, DxCheckBoxModule, DxGalleryModule, DxResponsiveBoxModule, DxTextAreaModule, DxTextBoxModule } from 'devextreme-angular';
import { DxiColModule, DxiLocationModule, DxiRowModule } from 'devextreme-angular/ui/nested';

@Component({
  templateUrl: 'home.component.html',
  styleUrls: [ './home.component.scss' ]
})

export class HomeComponent{
    slideshowDelay = 5000;
    images : string[] =[
        "../../../assets/logo/images2.jpg",
        "../../../assets/logo/images1.jpg",
        "../../../assets/logo/images3.jpg",
    
    ];
    screen(width) {
        return ( width < 700 ) ? 'sm' : 'lg';
    }
    constructor() {}
}
@NgModule({
    imports: [
        BrowserModule,
        DxCheckBoxModule,
        DxGalleryModule,
        DxButtonModule,
        DxBoxModule,
        DxiLocationModule,
        DxiColModule,
        DxiRowModule,
        DxResponsiveBoxModule
        
    ],
    declarations: [HomeComponent],
    bootstrap: [HomeComponent]
})
export class HomeModule { }