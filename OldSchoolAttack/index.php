<?php
error_reporting(E_NONE); // E_ALL
ini_set('display_errors', 0);

session_start();

// Query
$mysqli = new mysqli('192.168.1.5', 'root', 'MySuperSecretPassword', 'test', 3306);
if ($mysqli->connect_errno) die();

if(isset($_SESSION['wait']) && is_numeric($_SESSION['wait']))
{
  // Prevent brute force
  sleep($_SESSION['wait']);
}

$u=sha1(isset($_GET['u'])?$_GET['u']:'');
$p=sha1(isset($_GET['p'])?$_GET['p']:'');

// Check credentials
$resultado = $mysqli->query('SELECT 1 FROM users WHERE user="'.$u.'" AND pass="'.$p.'" LIMIT 1');
//print_r($resultado);
if ($resultado===false || $resultado->num_rows !== 1)
{
  // Prevent brute force
  $wait=1;
  if(isset($_SESSION['wait']) && is_numeric($_SESSION['wait'])) $wait=$_SESSION['wait']+1;
  $_SESSION['wait']=$wait>3?3:$wait; 
}
else
{
  $row = $resultado->fetch_array();
  $resultado->free();

  // Login
  if ($row!=NULL && $row[0]==1)
  {
    unset($_SESSION['wait']);
    echo 'SUPER SECRET CONTENT';
    die();
  }
}
$mysqli->close();