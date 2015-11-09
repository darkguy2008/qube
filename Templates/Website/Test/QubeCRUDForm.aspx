<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="QubeCRUDForm.aspx.cs" Inherits="Test_QubeCRUDForm" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mHead" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mContent" Runat="Server">

    <q:QubeCRUDForm2 runat="server" ID="frmCRUD"

        DataQueryStringEditField="id"
        DataQueryStringNewField="new"
        DataQueryStringDeleteField="del"
        
        IncludeValidationSummary="true"
        LabelWidth="150px"
        
        SwitchMode="QueryString"

    >

        <q:QubeCRUDFormPanel2 runat="server" ID="pRead" Types="Read">
            <p>Read mode!</p>
        </q:QubeCRUDFormPanel2>

        <q:QubeCRUDFormPanel2 runat="server" ID="pEdit" Types="Create, Update">
            <p>Edit mode!</p>
        </q:QubeCRUDFormPanel2>

        <q:QubeCRUDFormPanel2 runat="server" ID="pDelete" Types="Delete">
            <p>Delete?</p>
        </q:QubeCRUDFormPanel2>

    </q:QubeCRUDForm2>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="mJS" Runat="Server">
</asp:Content>
