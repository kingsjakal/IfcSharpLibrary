
// ifc_model.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace ifc{//==============================

public partial class Repository{//==========================================================================================

public static ifc.Model CurrentModel=new ifc.Model();

}//========================================================================================================


public partial class Model{//==========================================================================================
public               Model(){}
public               Model(string name){this.Header.name=name;}


public int NextGlobalId=1;
public int NextGlobalCommentId=0;
public ifc.HeaderData Header=new ifc.HeaderData();
public List<ENTITY> EntityList=new List<ENTITY>();

public void ClearEntityList(){EntityList.Clear();Header.Reset();}
public Dictionary<int,ENTITY> EntityDict=new Dictionary<int,ENTITY>();

public void AssignEntities()
{
EntityDict.Clear();
foreach (ENTITY e in EntityList) if (e.Id>0) if (!EntityDict.ContainsKey(e.Id))  {EntityDict.Add(e.Id,e);} else Console.WriteLine("#"+e.Id+" already exist! (double Entry)");
foreach (ENTITY e in EntityList) if (e.Id>0)
        {//####################################################################################################
         Dictionary<int,FieldInfo> VarDict=new Dictionary<int,FieldInfo>();
         int VarCount=0; foreach (FieldInfo field in e.GetType().GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) foreach (Attribute attr in field.GetCustomAttributes(true)) if (attr is ifcAttribute) {VarDict.Add(((ifcAttribute)attr).OrdinalPosition,field);VarCount++;} 
         for (int i=1;i<=VarCount;i++)       
             {//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
              FieldInfo field=VarDict[i];

                   if ( field.FieldType.IsSubclassOf(typeof(ENTITY))) 
                      {ENTITY E=(ENTITY)field.GetValue(e);
                       if (E!=null) {if (E.Id>0) if (EntityDict.ContainsKey(E.Id)) field.SetValue(e,EntityDict[E.Id]); /* E=EntityDict[E.Id];*/  
                                     else Console.WriteLine("E.Id="+E.Id+" nicht gefunden"); 
                                    } 
                      }
              else if (field.FieldType.IsSubclassOf(typeof(SELECT))) 
                      {
                       SELECT S=(SELECT)field.GetValue(e);
                       if (S!=null)
                          {//...........................................
                           if   (S.Id>0 && EntityDict.ContainsKey(S.Id)) S.SetValueAndType(EntityDict[S.Id],EntityDict[S.Id].GetType()); 
                           else if (!S.IsNull) {ENTITY E=null; if (S!=null)   if ( S.SelectType().IsSubclassOf(typeof(ENTITY)) ) E=(ENTITY)S.SelectValue(); 
                                                if (E!=null) if (E.Id>0 && EntityDict.ContainsKey(E.Id)) S.SetValueAndType(EntityDict[E.Id],EntityDict[E.Id].GetType()); 
                                               }
                           }//...........................................

                      }
              else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType)) if (field.GetValue(e)!=null) 
                      {//==================================================================
                       //Console.WriteLine("start list "+i+":"+field.FieldType.Name);
                       Dictionary<int,object> VarDict1=new Dictionary<int,object>();
                       int VarCount1=0;foreach (object item in (IEnumerable)field.GetValue(e)) if (item!=null) VarDict1.Add(VarCount1++,item);
                       object[] FieldCtorArgs=new object[VarCount1];
                       Type GenericType=null;
                       if (field.FieldType.BaseType.GetGenericArguments().Length>0) GenericType=field.FieldType.BaseType.GetGenericArguments()[0]; //LengthMeasure or CartesianPoint
                       else                                                         GenericType=field.FieldType.BaseType.BaseType.GetGenericArguments()[0]; //CompoundPlaneAngleMeasure
                       if ((GenericType!=null) &&  ( (GenericType.IsSubclassOf(typeof(ENTITY))) || GenericType.IsSubclassOf(typeof(SELECT))  ) )
                          {//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                           for (int i1=0;i1<VarCount1;i1++)       
                               {//------------------------------------------------------
                                object item=VarDict1[i1]; //Console.Write(field.Name+", "+i+" "+i1);
                                     if (item is SELECT) {//Console.WriteLine("SELECT item "+((SELECT)item).Id +" "+((SELECT)item).SelectType().Name); 

                                                           if (((SELECT)item).Id==0) if ( ((SELECT)item).SelectType().IsSubclassOf(typeof(ENTITY)) ) { ((SELECT)item).Id=((ENTITY)((SELECT)item).SelectValue()).Id; } 

                                                           if (((SELECT)item).Id>0) {//SELECT s=new SELECT(); /*((SELECT)item)*/ 
                                                                                      SELECT s=(SELECT)item;
                                                                                       s.SetValueAndType(EntityDict[((SELECT)item).Id],EntityDict[((SELECT)item).Id].GetType()); 
                                                                                       FieldCtorArgs[i1]=s;// Console.WriteLine(GenericType.Name+": ");

                                                                                    }
                                                           
                                                         }
                                else if (item is ENTITY) {//===================
                                                          if (((ENTITY)item).Id>0) 
                                                             {ENTITY E=(ENTITY)item; // Console.WriteLine("((ENTITY)item).Id="+((ENTITY)item).Id );
                                                              if (E!=null) if (E.Id>0) {//........................
                                                                                        if (EntityDict.ContainsKey(E.Id)) E=EntityDict[E.Id];  else Console.WriteLine("E.Id="+E.Id+" nicht gefunden");}
                                                                                        FieldCtorArgs[i1]=E;       
                                                                                       }//........................
                                                         }//===================
                               }//---------------------------------------------------
                            field.SetValue(e,Activator.CreateInstance(field.FieldType,FieldCtorArgs)); // ERROR !!
 
                          }//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                //       Console.WriteLine("end list");
                      }//==============================================================
             }//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++// of foreach field
        }//#################################################################################################### //of foreach Entity
}//of void

}//========================================================================================================








}// ifc=======================================